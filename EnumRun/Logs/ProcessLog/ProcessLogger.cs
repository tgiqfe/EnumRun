using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumRun.Lib;
using LiteDB;
using EnumRun.Logs;
using EnumRun.Lib.Syslog;
using System.Diagnostics;
using EnumRun.ScriptDelivery;

namespace EnumRun.Logs.ProcessLog
{
    /// <summary>
    /// ProcessLog送信用クラス
    /// </summary>
    internal class ProcessLogger : LoggerBase<ProcessLogBody>
    {
        protected override bool _logAppend { get { return true; } }
        protected override string _tag { get { return ProcessLogBody.TAG; } }

        private LogLevel _minLogLevel = LogLevel.Info;

        public ProcessLogger(EnumRunSetting setting, ScriptDeliverySession session)
        {
            _minLogLevel = LogLevelMapper.ToLogLevel(setting.MinLogLevel);

            Init(Item.ProcessName, setting, session);

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

        /// <summary>
        /// ログ出力 (レベル:Info, タイトル:無し)
        /// </summary>
        /// <param name="message"></param>
        public void Write(string message)
        {
            Write(LogLevel.Info, null, message);
        }

        #endregion

        public override async Task CloseAsync()
        {
            Write("終了");

            await base.CloseAsync();
        }
    }
}
