using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using EnumRun.Lib;
using EnumRun.Lib.Syslog;
using System.IO;

namespace EnumRun.Logs
{
    internal class LoggerBase : IDisposable
    {
        protected string _logDir = null;
        protected StreamWriter _writer = null;
        protected AsyncLock _lock = null;           //  [案] Lockは全体共有化する。今はProcessLog,MachineLog,SessionLogでそれぞれ別管理になっている。
        protected TransportLogstash _logstash = null;
        protected TransportSyslog _syslog = null;
        protected TransportDynamicLog _dynamicLog = null;
        protected LiteDatabase _liteDB = null;

        protected virtual bool _logAppend { get; }

        #region LiteDB methods

        protected LiteDatabase GetLiteDB()
        {
            string dbPath = Path.Combine(
                _logDir,
                "Cache_" + DateTime.Now.ToString("yyyyMMdd") + ".db");
            return new LiteDatabase($"Filename={dbPath};Connection=shared");
        }

        protected ILiteCollection<T> GetCollection<T>(string tableName) where T : LogBodyBase
        {
            var collection = _liteDB.GetCollection<T>(tableName);
            collection.EnsureIndex(x => x.Serial, true);
            return collection;
        }

        #endregion


        protected async Task Send<T>(T body) where T : LogBodyBase
        {
            using (await _lock.LockAsync())
            {
                string json = body.GetJson();

                //ファイル書き込み
                await _writer.WriteLineAsync(json);





            }
        }

        /// <summary>
        /// 一度ログ転送に失敗してローカルキャッシュしたログを、再転送
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheDB"></param>
        /// <param name="name"></param>
        /// <param name="setting"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public async Task ResendAsync<T>(LiteDatabase cacheDB, string name, EnumRunSetting setting, ScriptDelivery.ScriptDeliverySession session) where T : LogBodyBase
        {
            if (!name.Contains("_")) { return; }

            string transport = name.Substring(name.IndexOf("_"));
            var col = cacheDB.GetCollection<T>();
            col.EnsureIndex(x => x.Serial, true);
            IEnumerable<T> tempLogs = col.FindAll();
            switch (transport)
            {
                case "_logstash":
                    _logstash ??= new TransportLogstash(setting.Logstash.Server);
                    if (_logstash.Enabled)
                    {
                        cacheDB.DropCollection(name);
                        foreach (var body in tempLogs)
                        {
                            bool res = false;
                            res = await _logstash.SendAsync(body.GetJson());
                            if (!res)
                            {
                                col.Upsert(body);
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
                        _syslog.ProcId = ProcessLog.ProcessLogBody.TAG;
                    }
                    if (_syslog.Enabled)
                    {
                        cacheDB.DropCollection(name);
                        foreach (var body in tempLogs)
                        {
                            foreach (var pair in body.SplitForSyslog())
                            {
                                await _syslog.SendAsync(body.Level, pair.Key, pair.Value);
                            }
                        }
                    }
                    break;
                case "_dynamicLog":
                    _dynamicLog ??= new TransportDynamicLog(session, "DynamicLog");
                    if (_dynamicLog.Enabled)
                    {
                        cacheDB.DropCollection(name);
                        foreach (var body in tempLogs)
                        {
                            bool res = await _dynamicLog.SendAsync(body.GetJson());
                            if (!res)
                            {
                                col.Upsert(body);
                            }
                        }
                    }
                    break;
            }
        }



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
