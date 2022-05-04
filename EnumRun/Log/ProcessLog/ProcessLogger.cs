﻿using EnumRun.Lib;
using System.Text;
using LiteDB;
using EnumRun.Log;
using EnumRun.Lib.Syslog;
using System.Diagnostics;

namespace EnumRun.Log.ProcessLog
{
    internal class ProcessLogger : IDisposable
    {
        private string _logPath = null;
        private LogLevel _minLogLevel = LogLevel.Info;
        private StreamWriter _writer = null;
        private ReaderWriterLock _rwLock = null;

        private LogstashTransport _logstash = null;
        private SyslogTransport _syslog = null;
        private LiteDatabase _liteDB = null;
        private ILiteCollection<ProcessLogBody> _logstashCollection = null;
        private ILiteCollection<ProcessLogBody> _syslogCollection = null;

        /// <summary>
        /// 引数無しコンストラクタ
        /// </summary>
        public ProcessLogger() { }

        public ProcessLogger(EnumRunSetting setting)
        {
            _logPath = Path.Combine(
                setting.LogsPath,
                $"{Item.ProcessName}_{DateTime.Now.ToString("yyyyMMdd")}.log");
            TargetDirectory.CreateParent(_logPath);

            _minLogLevel = LogLevelMapper.ToLogLevel(setting.MinLogLevel);
            _writer = new StreamWriter(_logPath, true, new UTF8Encoding(false));
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
                    if (_liteDB == null)
                    {
                        string localDBPath = Path.Combine(
                            Path.GetDirectoryName(_logPath),
                            "LocalDB_" + DateTime.Now.ToString("yyyyMMdd") + ".db");
                        _liteDB = new LiteDatabase($"Filename={localDBPath};Connection=shared");
                    }
                    if (_logstashCollection == null)
                    {
                        _logstashCollection = _liteDB.GetCollection<ProcessLogBody>(ProcessLogBody.TAG + "_logstash");
                        _logstashCollection.EnsureIndex(x => x.Serial, true);
                    }
                    _logstashCollection.Upsert(body);
                }

                //  Syslog転送
                //  事前の接続可否チェック(コンストラクタ実行時)で導通不可の場合にローカルDBへ格納
                if (_syslog?.Enabled ?? false)
                {
                    await _syslog.WriteAsync(body.Level, body.ScriptFile, body.Message);
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
                        _syslogCollection = _liteDB.GetCollection<ProcessLogBody>(ProcessLogBody.TAG + "_syslog");
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
            Write("終了");

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