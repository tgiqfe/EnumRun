using ScriptDelivery.Lib;
using System.Text;
using LiteDB;
using ScriptDelivery.Lib.Syslog;
using System.Diagnostics;

namespace ScriptDelivery.Logs.ServerLog
{
    internal class ServerLogger : LoggerBase<ServerLogBody>
    {
        protected override bool _logAppend { get { return true; } }
        protected override string _tag { get { return ServerLogBody.TAG; } }
        private LogLevel _minLogLevel = LogLevel.Info;

        public ServerLogger(Setting setting)
        {
            _minLogLevel = LogLevelMapper.ToLogLevel(setting.MinLogLevel);

            Init(Item.ProcessName, setting);

            //  定期的にログファイルを書き込むスレッドを開始
            WriteInFile();

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
    }
}
