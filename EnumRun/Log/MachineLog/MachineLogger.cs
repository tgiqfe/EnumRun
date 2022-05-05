using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using EnumRun.Lib;
using EnumRun.Lib.Syslog;

namespace EnumRun.Log.MachineLog
{
    internal class MachineLogger : IDisposable
    {
        private string _logPath = null;
        private StreamWriter _writer = null;
        private ReaderWriterLock _rwLock = null;

        private LogstashTransport _logstash = null;
        private SyslogTransport _syslog = null;
        private LiteDatabase _liteDB = null;
        private ILiteCollection<MachineLogBody> _logstashCollection = null;
        private ILiteCollection<MachineLogBody> _syslogCollection = null;

        /// <summary>
        /// 引数無しコンストラクタ
        /// </summary>
        public MachineLogger() { }

        public MachineLogger(EnumRunSetting setting)
        {
            _logPath = Path.Combine(
                setting.GetLogsPath(),
                $"MachineLog_{DateTime.Now.ToString("yyyyMMdd")}.log");
            TargetDirectory.CreateParent(_logPath);

            _writer = new StreamWriter(_logPath, false, new UTF8Encoding(false));
            _rwLock = new ReaderWriterLock();

            if (!string.IsNullOrEmpty(setting.Logstash?.Server))
            {
                _logstash = new LogstashTransport(setting.Logstash.Server);
            }
            if (!string.IsNullOrEmpty(setting.Syslog?.Server))
            {
                _syslog = new SyslogTransport(setting);
                _syslog.Facility = FacilityMapper.ToFacility(setting.Syslog.Facility);
                _syslog.AppName = Item.ProcessName;
                _syslog.ProcId = MachineLogBody.TAG;
            }
        }

        public void Write()
        {
            SendAsync(new MachineLogBody(init: true)).ConfigureAwait(false);
        }

        private async Task SendAsync(MachineLogBody body)
        {
            try
            {
                _rwLock.AcquireWriterLock(10000);

                string json = body.GetJson();

                Console.WriteLine(json);

                //ファイル書き込み
                await _writer.WriteLineAsync(json);

                //  Logstash転送
                bool res = false;
                if (_logstash?.Enabled ?? false)
                {
                    res = await _logstash.SendAsync(json);
                }
                if (!res)
                {
                    if (_liteDB == null)
                    {
                        string localDBPath = Path.Combine(
                            Path.GetDirectoryName(_logPath),
                            "LocalDB_" + DateTime.Now.ToString("yyyyMMdd") + ".db");
                        _liteDB = new LiteDatabase($"Filename={localDBPath};Connection=shared");
                    }
                    if (_logstashCollection == null)
                    {
                        _logstashCollection = _liteDB.GetCollection<MachineLogBody>(MachineLogBody.TAG + "_logstash");
                        _logstashCollection.EnsureIndex(x => x.Serial, true);
                    }
                    _logstashCollection.Upsert(body);
                }

                //  Syslog転送
                if (_syslog?.Enabled ?? false)
                {
                    foreach (var pair in body.GetSyslogMessage())
                    {
                        await _syslog.SendAsync(LogLevel.Info, pair.Key, pair.Value);
                    }
                }
                else
                {
                    if (_liteDB == null)
                    {
                        string localDBPath = Path.Combine(
                            Path.GetDirectoryName(_logPath),
                            "LocalDB_" + DateTime.Now.ToString("yyyyMMdd") + ".db");
                        _liteDB = new LiteDatabase($"Filename={localDBPath};Connection=shared");
                    }
                    if (_syslogCollection == null)
                    {
                        _syslogCollection = _liteDB.GetCollection<MachineLogBody>(MachineLogBody.TAG + "_syslog");
                        _syslogCollection.EnsureIndex(x => x.Serial, true);
                    }
                    _syslogCollection.Upsert(body);
                }
            }
            catch { }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        public void Close()
        {

            //  一応最大1000ミリ秒待機
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
