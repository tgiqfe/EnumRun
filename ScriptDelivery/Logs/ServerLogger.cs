using ScriptDelivery.Lib;
using System.Text;
using LiteDB;
using ScriptDelivery.Logs;
using ScriptDelivery.Lib.Syslog;
using System.Diagnostics;

namespace ScriptDelivery.Logs
{
    internal class ServerLogger : LoggerBase
    {
        protected override bool _logAppend { get { return true; } }

        private LogLevel _minLogLevel = LogLevel.Info;
        //private ILiteCollection<ProcessLogBody> _logstashCollection = null;
        private ILiteCollection<ServerLogBody> _syslogCollection = null;

        public ServerLogger(Setting setting)
        {
            string logFileName =
                $"ScriptDelivery_{DateTime.Now.ToString("yyyyMMdd")}.log";
            string logPath = Path.Combine(setting.GetLogsPath(), logFileName);
            TargetDirectory.CreateParent(logPath);

            _logDir = setting.GetLogsPath();
            _writer = new StreamWriter(logPath, _logAppend, Encoding.UTF8);
            _rwLock = new ReaderWriterLock();
            _minLogLevel = LogLevelMapper.ToLogLevel(setting.MinLogLevel);

            if (!string.IsNullOrEmpty(setting.Syslog?.Server))
            {
                _syslog = new SyslogTransport(setting);
                _syslog.Facility = FacilityMapper.ToFacility(setting.Syslog.Facility);
                _syslog.AppName = Item.ProcessName;
                _syslog.ProcId = ServerLogBody.TAG;
            }

            Write("開始");
        }

        #region Log output

        public void Write(LogLevel level, string address, string title, string message)
        {
            if (level >= _minLogLevel)
            {
                SendAsync(new ServerLogBody(init: true)
                {
                    Date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                    Level = level,
                    ClientAddress = address,
                    Title = title,
                    Message = message,
                }).ConfigureAwait(false);
            }
        }

        public void Write(LogLevel level, string address, string title, string format, params string[] args)
        {
            Write(level, address, title, string.Format(format, args));
        }

        public void Write(string message)
        {
            Write(LogLevel.Info, address: null, title: "", message);
        }

        #endregion

        private async Task SendAsync(ServerLogBody body)
        {
            try
            {
                _rwLock.AcquireWriterLock(10000);

                //  コンソール出力
                Console.WriteLine("[{0}][{1}] Client:{2} Title:{3} Message:{4}",
                    body.Date,
                    body.Level,
                    body.ClientAddress,
                    body.Title,
                    body.Message);

                //  ファイル書き込み
                string json = body.GetJson();
                await _writer.WriteLineAsync(json);

                //  Syslog転送
                if (_syslog?.Enabled ?? false)
                {
                    await _syslog.SendAsync(body.Level, body.Title, body.Message);
                }
                else
                {
                    _liteDB ??= GetLiteDB();
                    _syslogCollection ??= GetCollection<ServerLogBody>(ServerLogBody.TAG + "_syslog");
                    _syslogCollection.Upsert(body);
                }
            }
            catch { }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        public override void Close()
        {
            Write("終了");

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
    }
}
