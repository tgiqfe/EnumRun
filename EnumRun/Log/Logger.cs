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

        /*
        /// <summary>
        /// 動的パラメータ付きコンストラクタ。
        /// ファイル名のパーツをパーツのまま指定するときに使用
        /// </summary>
        /// <param name="logDirectory"></param>
        /// <param name="format"></param>
        /// <param name="fileNameParts"></param>
        public Logger(string logDirectory, string format, params string[] fileNameParts) :
            this(logDirectory, string.Format(format, fileNameParts))
        {
        }

        /// <summary>
        /// ファイル名を指定するコンストラクタ
        /// </summary>
        /// <param name="logDirectory"></param>
        /// <param name="logFileName"></param>
        public Logger(string logDirectory, string logFileName)
        {
            this._logPath = Path.Combine(logDirectory, logFileName);
            ParentDirectory.Create(_logPath);
            _writer = new StreamWriter(_logPath, true, new UTF8Encoding(false));
            _body = new LogBody();
            _body.Init();
        }
        */

        public Logger(EnumRunSetting setting)
        {
            _logPath = Path.Combine(
                setting.LogsPath, 
                $"{Item.ProcessName}_{DateTime.Now.ToString("yyyyMMdd")}.log");
            ParentDirectory.Create(_logPath);
            _writer = new StreamWriter(_logPath, true, new UTF8Encoding(false));
            _body = new LogBody();
            _body.Init();
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






        #region Dispose

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_writer != null)
                    {
                        _writer.Dispose();
                    }
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
