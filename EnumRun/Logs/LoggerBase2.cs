﻿using System;
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
        private string _logDBPath = null;
        private TransportLogstash _logstash = null;
        private TransportSyslog _syslog = null;
        private TransportDynamicLog _dynamicLog = null;

        private ILiteCollection<T> _colLogstash = null;
        private ILiteCollection<T> _colSyslog = null;
        private ILiteCollection<T> _colDynamicLog = null;

        //protected string _logDir = null;
        protected virtual bool _logAppend { get; }
        protected virtual string _tag { get; set; }
        

        public void Init(string logFileName, EnumRunSetting setting, ScriptDeliverySession session)
        {
            _lock ??= new AsyncLock();

            string logDir = setting.GetLogsPath();
            string logFilePath = Path.Combine(logDir, logFileName);
            TargetDirectory.CreateParent(logFilePath);
            _writer = new StreamWriter(logFilePath, _logAppend, Encoding.UTF8);
            _logDBPath = Path.Combine(
                logDir,
                "Cache_" + DateTime.Now.ToString("yyyyMMdd") + ".db");



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
                _dynamicLog = new TransportDynamicLog(session, "ProcessLog");
            }
        }

        #region LiteDB methods

        protected LiteDatabase GetLiteDB()
        {
            /*
            string dbPath = Path.Combine(
                _logDir,
                "Cache_" + DateTime.Now.ToString("yyyyMMdd") + ".db");
            return new LiteDatabase($"Filename={dbPath};Connection=shared");
            */
            return new LiteDatabase($"Filename={_logDBPath};Connection=shared");
        }

        protected ILiteCollection<T> GetCollection(string tableName)
        {
            var collection = _liteDB.GetCollection<T>(tableName);
            collection.EnsureIndex(x => x.Serial, true);
            return collection;
        }

        #endregion

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
                        _liteDB ??= GetLiteDB();
                        _colLogstash ??= GetCollection(_tag + "_logstash");
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
                        _liteDB ??= GetLiteDB();
                        _colSyslog ??= GetCollection(_tag + "_syslog");
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
                        _liteDB ??= GetLiteDB();
                        _colDynamicLog ??= GetCollection(_tag + "_dynamicLog");
                        _colDynamicLog.Upsert(body);
                    }
                }
            }
        }

        #region Resend

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
