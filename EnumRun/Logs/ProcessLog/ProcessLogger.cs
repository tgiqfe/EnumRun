using EnumRun.Lib;
using System.Text;
using LiteDB;
using EnumRun.Logs;
using EnumRun.Lib.Syslog;
using System.Diagnostics;

namespace EnumRun.Logs.ProcessLog
{
    internal class ProcessLogger : LoggerBase
    {
        protected override bool _logAppend { get { return true; } }

        private LogLevel _minLogLevel = LogLevel.Info;
        private ILiteCollection<ProcessLogBody> _logstashCollection = null;
        private ILiteCollection<ProcessLogBody> _syslogCollection = null;

        //public ProcessLogger() { }

        public ProcessLogger(EnumRunSetting setting)
        {
            string logFileName =
                $"{Item.ProcessName}_{DateTime.Now.ToString("yyyyMMdd")}.log";
            string logPath = Path.Combine(setting.GetLogsPath(), logFileName);
            TargetDirectory.CreateParent(logPath);

            _logDir = setting.GetLogsPath();
            _writer = new StreamWriter(logPath, _logAppend, Encoding.UTF8);
            _rwLock = new ReaderWriterLock();
            _minLogLevel = LogLevelMapper.ToLogLevel(setting.MinLogLevel);

            if (!string.IsNullOrEmpty(setting.Logstash?.Server))
            {
                _logstash = new LogstashTransport(setting.Logstash.Server);
            }
            if (!string.IsNullOrEmpty(setting.Syslog?.Server))
            {
                _syslog = new SyslogTransport(setting);
                _syslog.Facility = FacilityMapper.ToFacility(setting.Syslog.Facility);
                _syslog.AppName = Item.ProcessName;
                _syslog.ProcId = ProcessLogBody.TAG;
            }

            Write("開始");
        }

        #region Log output

        /// <summary>
        /// ログ出力
        /// </summary>
        /// <param name="level"></param>
        /// <param name="scriptFile"></param>
        /// <param name="message"></param>
        public void Write(LogLevel level, string scriptFile, string message)
        {
            if (level >= _minLogLevel)
            {
                SendAsync(new ProcessLogBody(init: true)
                {
                    Date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                    Level = level,
                    ScriptFile = scriptFile,
                    Message = message,
                }).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// ログ出力 (strign.Format対応)
        /// </summary>
        /// <param name="level"></param>
        /// <param name="scriptFile"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Write(LogLevel level, string scriptFile, string format, params object[] args)
        {
            Write(level, scriptFile, string.Format(format, args));
        }

        /// <summary>
        /// ログ出力 (スクリプトファイル:無し)
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        public void Write(LogLevel level, string message)
        {
            Write(level, null, message);
        }

        /// <summary>
        /// ログ出力 (レベル:Info, スクリプトファイル:無し)
        /// </summary>
        /// <param name="message"></param>
        public void Write(string message)
        {
            Write(LogLevel.Info, null, message);
        }

        #endregion

        private async Task SendAsync(ProcessLogBody body)
        {
            try
            {
                _rwLock.AcquireWriterLock(10000);

                string json = body.GetJson();

                //ファイル書き込み
                await _writer.WriteLineAsync(json);

                //  Logstash転送
                //  事前の接続可否チェック(コンストラクタ実行時)で導通不可、あるいは、
                //  ログ転送時のResponseでHTTPResult:200でない場合にローカルDBへ格納
                bool res = false;
                if (_logstash?.Enabled ?? false)
                {
                    res = await _logstash.SendAsync(json);
                }
                if (!res)
                {
                    _liteDB ??= GetLiteDB();
                    _logstashCollection ??= GetCollection<ProcessLogBody>(ProcessLogBody.TAG + "_logstash");

                    _logstashCollection.Upsert(body);
                }

                //  Syslog転送
                //  事前の接続可否チェック(コンストラクタ実行時)で導通不可の場合にローカルDBへ格納
                if (_syslog?.Enabled ?? false)
                {
                    await _syslog.SendAsync(body.Level, body.ScriptFile, body.Message);
                }
                else
                {
                    _liteDB ??= GetLiteDB();
                    _syslogCollection ??= GetCollection<ProcessLogBody>(ProcessLogBody.TAG + "_syslog");
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
