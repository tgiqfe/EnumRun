using ScriptDelivery.Lib;
using System.Text;
using LiteDB;
using ScriptDelivery.Lib.Syslog;
using System.Diagnostics;

namespace ScriptDelivery.Logs.ServerLog
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
            _lock = new AsyncLock();
            _minLogLevel = LogLevelMapper.ToLogLevel(setting.MinLogLevel);

            if (!string.IsNullOrEmpty(setting.Syslog?.Server))
            {
                _syslog = new SyslogTransport(setting);
                _syslog.Facility = FacilityMapper.ToFacility(setting.Syslog.Facility);
                _syslog.AppName = Item.ProcessName;
                _syslog.ProcId = ServerLogBody.TAG;
            }

            //  定期的にログファイルを書き込むスレッドを開始
            WriteInFile(logPath);

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
                    Client = address,
                    Title = title,
                    Message = message,
                }).ConfigureAwait(false);
            }
        }

        public void Write(LogLevel level, string address, string title, string format, params object[] args)
        {
            Write(level, address, title, string.Format(format, args));
        }

        public void Write(LogLevel level, string title, string message)
        {
            Write(level, null, title, message);
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
                using (await _lock.LockAsync())
                {
                    //  コンソール出力
                    Console.WriteLine("[{0}][{1}] Client:{2} Title:{3} Message:{4}",
                        body.Date,
                        body.Level,
                        body.Client ?? "-",
                        body.Title ?? "-",
                        body.Message);

                    //  ファイル書き込み
                    string json = body.GetJson();
                    await _writer.WriteLineAsync(json);

                    //  Syslog転送
                    if (_syslog != null)
                    {
                        if (_syslog.Enabled)
                        {
                            await _syslog.SendAsync(body.Level, body.Title, body.Message);
                        }
                        else
                        {
                            _liteDB ??= GetLiteDB("ScriptDelivery");
                            _syslogCollection ??= GetCollection<ServerLogBody>(ServerLogBody.TAG + "_syslog");
                            _syslogCollection.Upsert(body);
                        }
                    }

                    _writed = true;
                }
            }
            catch { }
        }

        /// <summary>
        /// 定期的にログをファイルに書き込む
        /// </summary>
        /// <param name="logPath"></param>
        private async void WriteInFile(string logPath)
        {
            while (true)
            {
                await Task.Delay(60 * 1000);
                if (_writed)
                {
                    try
                    {
                        using (await _lock.LockAsync())
                        {
                            _writer.Dispose();
                            _writer = new StreamWriter(logPath, _logAppend, Encoding.UTF8);
                            _writed = false;
                        }
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// クローズ処理
        /// </summary>
        /// <returns></returns>
        public override async Task CloseAsync()
        {
            Write("終了");

            using (await _lock.LockAsync())
            {
                base.Close();
            }
        }
    }
}
