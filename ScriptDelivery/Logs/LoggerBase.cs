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
    internal class LoggerBase : IDisposable
    {
        protected string _logDir = null;
        protected StreamWriter _writer = null;
        protected AsyncLock _lock = null;
        //protected LogstashTransport _logstash = null;
        protected SyslogTransport _syslog = null;
        protected LiteDatabase _liteDB = null;

        protected virtual bool _logAppend { get; }
        protected bool _writed = false;

        #region LiteDB methods

        protected LiteDatabase GetLiteDB(string preName)
        {
            string today = DateTime.Now.ToString("yyyyMMdd");
            string dbPath = Path.Combine(
                _logDir,
                $"{preName}_{today}.db");
            return new LiteDatabase($"Filename={dbPath};Connection=shared");
        }

        protected ILiteCollection<T> GetCollection<T>(string tableName) where T : LogBodyBase
        {
            var collection = _liteDB.GetCollection<T>(tableName);
            collection.EnsureIndex(x => x.Serial, true);
            return collection;
        }

        #endregion

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
