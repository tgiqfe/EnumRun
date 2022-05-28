using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using ScriptDelivery.Lib;
using ScriptDelivery.Lib.Syslog;
using System.IO;

namespace ScriptDelivery.Logs
{
    internal class LoggerBase2<T> :
        IDisposable
        where T : LogBodyBase
    {
        /// <summary>
        /// ログ記述用ロック。静的パラメータ
        /// </summary>
        private static AsyncLock _lock = null;

        private StreamWriter _writer = null;
        private LiteDatabase _liteDB = null;
        private string _liteDBPath = null;
        private TransportSyslog _syslog = null;

        private ILiteCollection<T> _colSyslog = null;

        protected virtual bool _logAppend { get; }
        protected virtual string _tag { get; set; }
        protected bool _writed = false;

        public void Init(string logPreName, Setting setting)
        {
            _lock ??= new AsyncLock();

            string logDir = setting.GetLogsPath();
            string today = DateTime.Now.ToString("yyyyMMdd");

            string logFilePath = Path.Combine(logDir, $"{logPreName}_{today}.log");
            TargetDirectory.CreateParent(logFilePath);
            _writer = new StreamWriter(logFilePath, _logAppend, Encoding.UTF8);
            _liteDBPath = Path.Combine(logDir, $"Cache_{today}.db");

            if (!string.IsNullOrEmpty(setting.Syslog?.Server))
            {
                _syslog = new TransportSyslog(setting);
                _syslog.Facility = FacilityMapper.ToFacility(setting.Syslog.Facility);
                _syslog.AppName = Item.ProcessName;
                _syslog.ProcId = _tag;
            }
        }

        private ILiteCollection<T> GetCollection(string tableName)
        {
            var collection = _liteDB.GetCollection<T>(tableName);
            collection.EnsureIndex(x => x.Serial, true);
            return collection;
        }

        #region Send

        public async Task SendAsync(T body)
        {
            using (await _lock.LockAsync())
            {
                //  コンソール出力
                Console.WriteLine(body.ToConsoleMessage());

                string json = body.GetJson();

                //  ファイル書き込み
                await _writer.WriteLineAsync(json);

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

                _writed = true;
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
