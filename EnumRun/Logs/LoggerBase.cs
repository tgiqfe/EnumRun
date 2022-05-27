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




        /*
        public async void Resend<T>(LiteDatabase cacheDB, string name) where T : LogBodyBase
        {
            if (!name.Contains("_")) { return; }

            string transport = name.Substring(name.IndexOf("_"));
            switch (transport)
            {
                case "_logstash":
                    _logstash ??= new TransportLogstash(setting.Logstash.Server);
                    if (_logstash.Enabled)
                    {
                        var col = cacheDB.GetCollection<T>();
                        IEnumerable<T> logs = col.FindAll();
                        cacheDB.DropCollection(name);

                        foreach (var body in logs)
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
                        _syslog.ProcId = ProcessLogBody.TAG;
                    }



                    break;
                case "_dynamicLog":
                    _dynamicLog ??= new TransportDynamicLog(session, "ProcessLog");
                    break;
            }
        }
        */


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
