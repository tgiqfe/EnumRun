﻿using System.Management;
using System.Reflection;
using System.Text;
using System.Text.Json;
using EnumRun.Lib;
using EnumRun.Logs;

namespace EnumRun
{
    internal class SessionWorker
    {
        public bool Enabled { get; set; }

        private EnumRunSetting _setting = null;

        private Logs.ProcessLog.ProcessLogger _logger = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="logger"></param>
        public SessionWorker(EnumRunSetting setting, Logs.ProcessLog.ProcessLogger logger)
        {
            _setting = setting;
            _logger = logger;
        }

        /// <summary>
        /// セッション開始時に実行
        /// </summary>
        public void PreProcess()
        {
            string logTitle = "PreProcess";

            //  前回セッション
            string filePath = TargetDirectory.GetFile(Item.SESSION_FILE);
            Dictionary<string, Logs.SessionLog.LogonSession> lastSessions = DeserializeLastLogonSession(filePath);

            //  今回セッション
            var body = new Logs.SessionLog.SessionLogBody();

            //  前回/今回セッションを比較し、実行可否チェック
            this.Enabled = RunnableCheck(
                lastSessions.ContainsKey(Item.ProcessName) ? lastSessions[Item.ProcessName] : null,
                body.Session);

            //  本日初回実行 (今回セッションで実行不可としても、本日初実行ならば実行)
            bool isTodayProcessed = lastSessions.Values.Any(x => DateTime.Today == x.ExecTime?.Date);
            if (!isTodayProcessed)
            {
                _logger.Write(LogLevel.Info, logTitle, "Today first.");

                //  MachineLogを出力
                using (var mLogger = new Logs.MachineLog.MachineLogger(_setting))
                {
                    mLogger.Write();
                }

                //  OldFileをクリア
                DeleteOldFile(_setting.GetLogsPath());
                DeleteOldFile(_setting.GetOutputPath());
            }

            //  SessionLogを出力
            using (var sLogger = new Logs.SessionLog.SessionLogger(_setting))
            {
                sLogger.Write(body);
            }

            //  セッション管理情報を出力
            lastSessions[Item.ProcessName] = body.Session;
            SerializeLogonSession(lastSessions, filePath);
        }

        /// <summary>
        /// セッション終了時に実行
        /// </summary>
        public void PostProcess()
        {
            //  [案]ここで外部サーバへログ転送失敗していたログキャッシュを、再転送する処理



            //  ScriptDeliverySessionと同時にDisposeした場合、最後の終了ログを出力する前に
            //  セッションが閉じてしまうことがある為、先に明示的にloggerをクローズ。
            _logger.CloseAsync().Wait();
        }

        /// <summary>
        /// 現在セッションについて、実行可否を確認
        /// </summary>
        /// <param name="lastSession"></param>
        /// <param name="currentSession"></param>
        /// <returns></returns>
        private bool RunnableCheck(Logs.SessionLog.LogonSession lastSession, Logs.SessionLog.LogonSession currentSession)
        {
            string logTitle = "RunnableCheck";

            bool ret = false;

            StringBuilder sb = new StringBuilder();
            if (lastSession == null)
            {
                _logger.Write(LogLevel.Debug, logTitle, "Last session is null.");
                ret = true;
            }
            else
            {
                bool rest = lastSession.ExecTime == null ?
                    true :
                    ((DateTime)currentSession.ExecTime - (DateTime)lastSession.ExecTime).TotalSeconds > (_setting.RestTime ?? 0);
                bool bootup = lastSession.BootupTime == currentSession.BootupTime;
                bool logon = lastSession.LogonTime == currentSession.LogonTime;
                bool id = lastSession.LogonId == currentSession.LogonId;

                if (rest)
                {
                    sb.Append("RestTime=Over");
                    ret = true;
                }
                else
                {
                    sb.Append("RestTime=NotOver");
                    sb.Append(string.Format(", BootupTime={0}, LogonTime={1}, LogonId={2}",
                        bootup ? "SameAsLast" : "Changed",
                        logon ? "SameAsLast" : "Changed",
                        id ? "SameAsLast" : "Changed"));
                    ret = !bootup && !logon && !id; ;
                }
            }
            _logger.Write(ret ? LogLevel.Info : LogLevel.Warn,
                logTitle,
                "Runnable => {0}, [{1}]",
                    ret ? "Enable" : "Disable",
                    sb.ToString());

            return ret;
        }

        /// <summary>
        /// 保持期間以上前のファイルを削除
        /// </summary>
        /// <param name="targetDirectory"></param>
        private void DeleteOldFile(string targetDirectory)
        {
            string logTitle = "DeleteOldFile";

            int retention = _setting.RetentionPeriod ?? 0;

            if (retention > 0)
            {
                DateTime border = DateTime.Now.AddDays(retention * -1);
                var files = (Directory.Exists(targetDirectory) ?
                    Directory.GetFiles(targetDirectory) :
                    new string[] { }).
                        Where(x => new FileInfo(x).LastWriteTime < border).ToArray();
                if (files.Length > 0)
                {
                    _logger.Write(LogLevel.Info,
                        logTitle,
                        "Old file => [ target={0}, count={1} ]",
                            targetDirectory, files.Length);
                }
                try
                {
                    foreach (var target in files)
                    {
                        File.Delete(target);
                        _logger.Write(LogLevel.Debug, logTitle, "Delete => {0}", target);
                    }
                }
                catch
                {
                    _logger.Write(LogLevel.Warn, logTitle, "Delete failed.");
                }
            }
        }

        private async void SendCacheLog()
        {
            if (Directory.Exists(_setting.LogsPath))
            {
                foreach (string dbPath in Directory.GetFiles(_setting.LogsPath, "Cache_*.db"))
                {
                    using (var liteDB = new LiteDB.LiteDatabase($"Filename={dbPath};Connection=shared"))
                    {
                        foreach (string name in liteDB.GetCollectionNames())
                        {
                            if (name.StartsWith(Logs.ProcessLog.ProcessLogBody.TAG))
                            {
                                if (name.EndsWith("logstash"))
                                {
                                    TransportLogstash logstash = new TransportLogstash(_setting.Logstash.Server);
                                    if (logstash.Enabled)
                                    {
                                        var collection = liteDB.GetCollection<Logs.ProcessLog.ProcessLogBody>(name);
                                        foreach (var entry in collection.FindAll())
                                        {
                                            string json = entry.GetJson();
                                            bool ret = await logstash.SendAsync(json);
                                            if (ret)
                                            {
                                                collection.Delete()
                                            }
                                        }

                                    }
                                }
                                else if (name.EndsWith("syslog"))
                                {

                                }
                                else if (name.EndsWith("dynamicLog"))
                                {

                                }
                            }
                            else if (name.StartsWith(Logs.MachineLog.MachineLogBody.TAG))
                            {

                            }
                            else if (name.StartsWith(Logs.SessionLog.SessionLogBody.TAG))
                            {

                            }
                        }
                    }
                }
            }
        }

        #region Serialize/Deserialize

        /// <summary>
        /// セッション管理ファイルを読み込んでデシリアライズ
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private Dictionary<string, Logs.SessionLog.LogonSession> DeserializeLastLogonSession(string filePath)
        {
            Dictionary<string, Logs.SessionLog.LogonSession> sessions = null;
            try
            {
                using (var sr = new StreamReader(filePath, Encoding.UTF8))
                {
                    sessions =
                        JsonSerializer.Deserialize<Dictionary<string, Logs.SessionLog.LogonSession>>(sr.ReadToEnd());
                }
            }
            catch { }
            return sessions ?? new Dictionary<string, Logs.SessionLog.LogonSession>();
        }

        /// <summary>
        /// セッション管理ファイルへシリアライズして保存
        /// </summary>
        /// <param name="sessions"></param>
        /// <param name="filePath"></param>
        private void SerializeLogonSession(Dictionary<string, Logs.SessionLog.LogonSession> sessions, string filePath)
        {
            TargetDirectory.CreateParent(filePath);
            using (var sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                string json = JsonSerializer.Serialize(
                    sessions,
                    new JsonSerializerOptions() { WriteIndented = true });
                sw.WriteLine(json);
            }
        }

        #endregion
    }
}
