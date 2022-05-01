using EnumRun.Lib;
using System.Text;
using LiteDB;

namespace EnumRun.Log.ProcessLog
{
    internal class Logger : IDisposable
    {
        private string _logPath = null;
        private LogLevel _minLogLevel = LogLevel.Info;
        private StreamWriter _writer = null;
        private ReaderWriterLock _rwLock = null;

        private LogstashTransport _transport = null;
        private LiteDatabase _liteDB = null;
        private ILiteCollection<LogBody> _collection = null;


        /// <summary>
        /// 引数無しコンストラクタ
        /// </summary>
        public Logger() { }

        public Logger(EnumRunSetting setting)
        {
            _logPath = Path.Combine(
                setting.LogsPath,
                $"{Item.ProcessName}_{DateTime.Now.ToString("yyyyMMdd")}.log");
            TargetDirectory.CreateParent(_logPath);

            _minLogLevel = setting.MinLogLevel ?? LogLevel.Info;
            _writer = new StreamWriter(_logPath, true, new UTF8Encoding(false));
            _rwLock = new ReaderWriterLock();

            if (!string.IsNullOrEmpty(setting.LogstashServer))
            {
                _transport = new LogstashTransport(setting.LogstashServer);
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
                SendAsync(new LogBody(init: true)
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

        private async Task SendAsync(LogBody body)
        {
            try
            {
                _rwLock.AcquireWriterLock(10000);

                string json = body.GetJson();

                //ファイル書き込み
                await _writer.WriteLineAsync(json);

                //  Logstash転送
                if (_transport?.Enabled ?? false)
                {
                    bool res = await _transport.SendAsync(json);

                    if (!res! && _collection != null)
                    {
                        if (_liteDB == null)
                        {
                            string localDBPath = Path.Combine(
                                Path.GetDirectoryName(_logPath),
                                "Logstash_Test_" + DateTime.Now.ToString("yyyyMMdd") + ".log");
                            _liteDB = new LiteDatabase($"Filename={localDBPath};Connection=shared");
                            _collection = _liteDB.GetCollection<LogBody>(LogBody.TAG);
                            _collection.EnsureIndex(x => x.Serial, true);
                        }
                        _collection.Upsert(body);
                    }
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
            Write("終了");

            //  一応1000ミリ秒待機
            try
            {
                _rwLock.AcquireWriterLock(10000);
                _rwLock.ReleaseWriterLock();
            }
            catch { }

            if (_writer != null) { _writer.Dispose(); }
            if (_liteDB != null) { _liteDB.Dispose(); }
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
