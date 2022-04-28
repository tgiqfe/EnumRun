using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using EnumRun.Lib;

namespace EnumRun.Log
{
    internal class Logger : IDisposable
    {
        private string _logPath = null;
        private StreamWriter _writer = null;
        private LogBody _body = null;

        /// <summary>
        /// 引数無しコンストラクタ
        /// </summary>
        public Logger() { }

        public Logger(EnumRunSetting setting)
        {
            _logPath = Path.Combine(
                setting.LogsPath,
                $"{Item.ProcessName}_{DateTime.Now.ToString("yyyyMMdd")}.log");
            ParentDirectory.Create(_logPath);
            _writer = new StreamWriter(_logPath, true, new UTF8Encoding(false));
            _body = new LogBody();
            _body.Init();

            Write("開始");
        }

        /// <summary>
        /// ログ出力
        /// </summary>
        /// <param name="level"></param>
        /// <param name="scriptFile"></param>
        /// <param name="message"></param>
        public void Write(LogLevel level, string scriptFile, string message)
        {
            _writer.WriteLine(_body.GetLog(level, scriptFile, message));
        }

        /// <summary>
        /// ログ出力 (strign.Format対応)
        /// </summary>
        /// <param name="level"></param>
        /// <param name="scriptFile"></param>
        /// <param name="format"></param>
        /// <param name="messages"></param>
        public void Write(LogLevel level, string scriptFile, string format, params string[] messages)
        {
            _writer.WriteLine(_body.GetLog(level, scriptFile, string.Format(format, messages)));
        }

        /// <summary>
        /// ログ出力 (スクリプトファイル:無し)
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        public void Write(LogLevel level, string message)
        {
            _writer.WriteLine(_body.GetLog(level, null, message));
        }

        /// <summary>
        /// ログ出力 (レベル:Info, スクリプトファイル:無し)
        /// </summary>
        /// <param name="message"></param>
        public void Write(string message)
        {
            _writer.WriteLine(_body.GetLog(LogLevel.Info, null, message));
        }


        public void Close()
        {
            if (_writer != null)
            {
                Write("終了");
                _writer.Dispose();
            }
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
