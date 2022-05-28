using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using EnumRun.Lib;
using EnumRun.Lib.Syslog;
using EnumRun.ScriptDelivery;
using System.IO;

namespace EnumRun.Logs
{
    internal class LoggerBase<T> :
        IDisposable
        where T : LogBodyBase
    {
        /// <summary>
        /// ログ記述用ロック。静的パラメータ
        /// </summary>
        private static AsyncLock _lock = null;

        private string _logFilePath = null;
        private StreamWriter _writer = null;
        private LiteDatabase _liteDB = null;
        private string _liteDBPath = null;
        private TransportLogstash _logstash = null;
        private TransportSyslog _syslog = null;
        private TransportDynamicLog _dynamicLog = null;

        private ILiteCollection<T> _colLogstash = null;
        private ILiteCollection<T> _colSyslog = null;
        private ILiteCollection<T> _colDynamicLog = null;

        protected virtual bool _logAppend { get; }
        protected virtual string _tag { get; set; }

        public void Init(string logPreName, EnumRunSetting setting, ScriptDeliverySession session)
        {
            _lock ??= new AsyncLock();

            string logDir = setting.GetLogsPath();
            string today = DateTime.Now.ToString("yyyyMMdd");

            _logFilePath = Path.Combine(logDir, $"{logPreName}_{today}.log");
            TargetDirectory.CreateParent(_logFilePath);
            _writer = new StreamWriter(_logFilePath, _logAppend, Encoding.UTF8);
            _liteDBPath = Path.Combine(logDir, $"Cache_{today}.db");

            if (!string.IsNullOrEmpty(setting.Logstash?.Server))
            {
                _logstash = new TransportLogstash(setting.Logstash.Server);
            }
            if (!string.IsNullOrEmpty(setting.Syslog?.Server))
            {
                _syslog = new TransportSyslog(setting);
                _syslog.Facility = FacilityMapper.ToFacility(setting.Syslog.Facility);
                _syslog.AppName = Item.ProcessName;
                _syslog.ProcId = _tag;
            }
            if (session.EnableLogTransport)
            {
                _dynamicLog = new TransportDynamicLog(session, _tag);
            }
        }

        private ILiteCollection<T> GetCollection(string tableName)
        {
            var collection = _liteDB.GetCollection<T>(tableName);
            collection.EnsureIndex(x => x.Serial, true);
            return collection;
        }

        #region Send/Resend

        /// <summary>
        /// 通常ログ出力、ログ転送。
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task SendAsync(T body)
        {
            using (await _lock.LockAsync())
            {
                string json = body.GetJson();

                //  ファイル書き込み
                await _writer.WriteLineAsync(json);

                //  Logstash転送
                if (_logstash != null)
                {
                    bool res = false;
                    if (_logstash.Enabled)
                    {
                        res = await _logstash.SendAsync(json);
                    }
                    if (!res)
                    {
                        _liteDB ??= new LiteDatabase($"Filename={_liteDBPath};Connection=shared");
                        _colLogstash ??= GetCollection($"{_tag}_logstash");
                        _colLogstash.Upsert(body);
                    }
                }

                //  Syslog転送
                if (_syslog != null)
                {
                    if (_syslog.Enabled)
                    {
                        foreach (var pair in body.SplitForSyslog())
                        {
                            await _syslog.SendAsync(LogLevel.Info, pair.Key, pair.Value);
                        }
                    }
                    else
                    {
                        _liteDB ??= new LiteDatabase($"Filename={_liteDBPath};Connection=shared");
                        _colSyslog ??= GetCollection($"{_tag}_syslog");
                        _colSyslog.Upsert(body);
                    }
                }

                //  DynamicLog転送
                if (_dynamicLog != null)
                {
                    if (_dynamicLog.Enabled)
                    {
                        await _dynamicLog.SendAsync(json);
                    }
                    else
                    {
                        _liteDB ??= new LiteDatabase($"Filename={_liteDBPath};Connection=shared");
                        _colDynamicLog ??= GetCollection($"{_tag}_dynamicLog");
                        _colDynamicLog.Upsert(body);
                    }
                }
            }
        }

        /// <summary>
        /// ログ転送失敗時キャッシュの再転送
        /// </summary>
        /// <param name="cacheDB"></param>
        /// <param name="name"></param>
        /// <param name="setting"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public async Task ResendAsync(LiteDatabase cacheDB, string name, EnumRunSetting setting, ScriptDelivery.ScriptDeliverySession session)
        {
            if (!name.Contains("_")) { return; }

            var col = cacheDB.GetCollection<T>();
            col.EnsureIndex(x => x.Serial, true);
            IEnumerable<T> bodies = col.FindAll();

            switch (name.Substring(name.IndexOf("_")))
            {
                case "_logstash":
                    _logstash ??= new TransportLogstash(setting.Logstash.Server);
                    if (_logstash.Enabled)
                    {
                        cacheDB.DropCollection(name);
                        bool connected = true;
                        foreach (var body in bodies)
                        {
                            bool res = false;
                            if (connected)
                            {
                                res = await _logstash.SendAsync(body.GetJson());
                            }
                            if (!connected || !res)
                            {
                                col.Upsert(body);
                                connected = false;
                            }
                        }
                    }
                    break;
                case "_syslog":
                    if (_syslog == null)
                    {
                        _syslog = new TransportSyslog(setting);
                        _syslog.Facility = FacilityMapper.ToFacility(setting.Syslog.Facility);
                        _syslog.AppName = Item.ProcessName;
                        _syslog.ProcId = _tag;
                    }
                    if (_syslog.Enabled)
                    {
                        cacheDB.DropCollection(name);
                        foreach (var body in bodies)
                        {
                            foreach (var pair in body.SplitForSyslog())
                            {
                                await _syslog.SendAsync(body.Level, pair.Key, pair.Value);
                            }
                        }
                    }
                    break;
                case "_dynamicLog":
                    _dynamicLog ??= new TransportDynamicLog(session, _tag);
                    if (_dynamicLog.Enabled)
                    {
                        cacheDB.DropCollection(name);
                        bool connected = true;
                        foreach (var body in bodies)
                        {
                            bool res = false;
                            if (connected)
                            {
                                res = await _dynamicLog.SendAsync(body.GetJson());
                            }
                            if (!connected || !res)
                            {
                                col.Upsert(body);
                                connected = false;
                            }
                        }
                    }
                    break;
            }
        }

        #endregion
        #region Close method

        public virtual async Task CloseAsync()
        {
            using (await _lock.LockAsync())
            {
                Close();
            }
        }

        public virtual void Close()
        {
            if (_writer != null) { _writer.Dispose(); _writer = null; }
            if (_liteDB != null) { _liteDB.Dispose(); _liteDB = null; }
            if (_syslog != null) { _syslog.Dispose(); _syslog = null; }
        }

        #endregion
        #region Dispose

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Close();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
