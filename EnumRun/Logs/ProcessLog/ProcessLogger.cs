using EnumRun.Lib;
using System.Text;
using LiteDB;
using EnumRun.Logs;
using EnumRun.Lib.Syslog;
using System.Diagnostics;
using EnumRun.ScriptDelivery;

namespace EnumRun.Logs.ProcessLog
{
    internal class ProcessLogger : LoggerBase
    {
        protected override bool _logAppend { get { return true; } }

        private LogLevel _minLogLevel = LogLevel.Info;
        private ILiteCollection<ProcessLogBody> _logstashCollection = null;
        private ILiteCollection<ProcessLogBody> _syslogCollection = null;
        private ILiteCollection<ProcessLogBody> _dynamicLogCollection = null;


        public ProcessLogger(EnumRunSetting setting, EnumRun.ScriptDelivery.ScriptDeliverySession session)
        {
            string logFileName =
                $"{Item.ProcessName}_{DateTime.Now.ToString("yyyyMMdd")}.log";
            string logPath = Path.Combine(setting.GetLogsPath(), logFileName);
            TargetDirectory.CreateParent(logPath);

            _logDir = setting.GetLogsPath();
            _writer = new StreamWriter(logPath, _logAppend, Encoding.UTF8);
            _lock = new AsyncLock();
            _minLogLevel = LogLevelMapper.ToLogLevel(setting.MinLogLevel);

            if (!string.IsNullOrEmpty(setting.Logstash?.Server))
            {
                _logstash = new TransportLogstash(setting.Logstash.Server);
            }
            if (!string.IsNullOrEmpty(setting.Syslog?.Server))
            {
                _syslog = new TransportSyslog(setting);
                _syslog.Facility = FacilityMapper.ToFacility(setting.Syslog.Facility);
                _syslog.AppName = Item.ProcessName;
                _syslog.ProcId = ProcessLogBody.TAG;
            }
            if (session.EnableLogTransport)
            {
                _dynamicLog = new TransportDynamicLog(session, "ProcessLog");
            }

            Write("開始");
        }

        #region Log output

        /// <summary>
        /// ログ出力
        /// </summary>
        /// <param name="level"></param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        public void Write(LogLevel level, string title, string message)
        {
            if (level >= _minLogLevel)
            {
                SendAsync(new ProcessLogBody(init: true)
                {
                    Date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                    Level = level,
                    Title = title ?? "-",
                    Message = message,
                }).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// ログ出力 (strign.Format対応)
        /// </summary>
        /// <param name="level"></param>
        /// <param name="title"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Write(LogLevel level, string title, string format, params object[] args)
        {
            Write(level, title, string.Format(format, args));
        }

        /*
        /// <summary>
        /// ログ出力 (スクリプトファイル:無し)
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        public void Write(LogLevel level, string message)
        {
            Write(level, null, message);
        }
        */

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
                using (await _lock.LockAsync())
                {
                    string json = body.GetJson();

                    //ファイル書き込み
                    await _writer.WriteLineAsync(json);

                    //  Logstash転送
                    //  事前の接続可否チェック(コンストラクタ実行時)で導通不可、あるいは、
                    //  ログ転送時のResponseでHTTPResult:200でない場合にローカルDBへ格納
                    if (_logstash != null)
                    {
                        bool res = false;
                        if (_logstash.Enabled)
                        {
                            if (_logstash.Enabled)
                            {
                                res = await _logstash.SendAsync(json);
                            }
                            if (!res)
                            {
                                _liteDB ??= GetLiteDB();
                                _logstashCollection ??= GetCollection<ProcessLogBody>(ProcessLogBody.TAG + "_logstash");
                                _logstashCollection.Upsert(body);
                            }
                        }
                    }

                    //  Syslog転送
                    //  事前の接続可否チェック(コンストラクタ実行時)で導通不可の場合にローカルDBへ格納
                    if (_syslog != null)
                    {
                        if (_syslog.Enabled)
                        {
                            await _syslog.SendAsync(body.Level, body.Title, body.Message);
                        }
                        else
                        {
                            _liteDB ??= GetLiteDB();
                            _syslogCollection ??= GetCollection<ProcessLogBody>(ProcessLogBody.TAG + "_syslog");
                            _syslogCollection.Upsert(body);
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
                            _dynamicLogCollection ??= GetCollection<ProcessLogBody>(ProcessLogBody.TAG + "_dynamicLog");
                            _dynamicLogCollection.Upsert(body);
                        }
                    }
                }
            }
            catch { }
        }








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
