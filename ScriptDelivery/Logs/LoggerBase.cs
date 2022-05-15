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
        protected ReaderWriterLock _rwLock = null;
        //protected LogstashTransport _logstash = null;
        protected SyslogTransport _syslog = null;
        protected LiteDatabase _liteDB = null;

        protected virtual bool _logAppend { get; }

        #region LiteDB methods

        protected LiteDatabase GetLiteDB()
        {
            string dbPath = Path.Combine(
                _logDir,
                "LocalDB_" + DateTime.Now.ToString("yyyyMMdd") + ".db");
            return new LiteDatabase($"Filename={dbPath};Connection=shared");
        }

        protected ILiteCollection<T> GetCollection<T>(string tableName) where T : LogBodyBase
        {
            var collection = _liteDB.GetCollection<T>(tableName);
            collection.EnsureIndex(x => x.Serial, true);
            return collection;
        }

        #endregion

        public virtual void Close()
        {
            try
            {
                _rwLock.AcquireWriterLock(10000);
                _rwLock.ReleaseWriterLock();
            }
            catch { }

            if (_writer != null) { _writer.Dispose(); }
            if (_liteDB != null) { _liteDB.Dispose(); }
            if (_syslog != null) { _syslog.Dispose(); }
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
