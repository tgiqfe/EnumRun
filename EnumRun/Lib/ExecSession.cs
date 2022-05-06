using System.Management;
using System.Reflection;
using System.Text;
using System.Text.Json;
using EnumRun.Log;

namespace EnumRun.Lib
{
    internal class ExecSession
    {
        public bool Enabled { get; set; }

        private EnumRunSetting _setting = null;

        private EnumRun.Log.ProcessLog.ProcessLogger _logger = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="logger"></param>
        public ExecSession(EnumRunSetting setting, EnumRun.Log.ProcessLog.ProcessLogger logger)
        {
            this._setting = setting;
            this._logger = logger;
        }

        /// <summary>
        /// セッション開始時に実行
        /// </summary>
        public void PreProcess()
        {
            //  前回セッション
            string filePath = TargetDirectory.GetFile(Item.SESSION_FILE);
            Dictionary<string, EnumRun.Log.SessionLog.LogonSession> lastSessions = DeserializeLastLogonSession(filePath);

            //  今回セッション
            var body = new EnumRun.Log.SessionLog.SessionLogBody();

            //  前回セッションと比較して、実行可否チェック
            StringBuilder sb = new StringBuilder();
            var lastSession = lastSessions.ContainsKey(Item.ProcessName) ? lastSessions[Item.ProcessName] : null;
            var currentSession = body.Session;
            if (lastSession == null)
            {
                this.Enabled = true;
            }
            else
            {
                bool rest = ((DateTime)currentSession.ExecTime - (DateTime)lastSession.ExecTime).TotalSeconds <= (_setting.RestTime ?? 0);
                bool bootup = lastSession.BootupTime == currentSession.BootupTime;
                bool logon = lastSession.LogonTime == currentSession.LogonTime;
                bool id = lastSession.LogonId == currentSession.LogonId;

                if (rest)
                {
                    sb.Append("RestTime=Over");
                    this.Enabled = true;
                }
                else
                {
                    sb.Append("RestTime=NotOver");
                    sb.Append(string.Format(", BootupTime={0}, LogonTime={1}, LogonId={2}",
                        bootup ? "SameAsLast" : "Changed",
                        logon ? "SameAsLast" : "Changed",
                        id ? "SameAsLast" : "Changed"));
                    this.Enabled = !bootup && !logon && !id; ;
                }
            }
            _logger.Write(Enabled ? LogLevel.Info : LogLevel.Warn, null,
                "Runnable => {0}, [{1}]",
                Enabled ? "Enable" : "Disable",
                sb.ToString());

            //  本日初回実行
            bool todayFirst = !(lastSessions.Values.
                Where(x => DateTime.Today == x.ExecTime?.Date).
                Any(x => x.TodayFirst ?? false));
            if (todayFirst)
            {
                _logger.Write(LogLevel.Info, "Today first.");
                body.Session.TodayFirst = true;

                //  MachineLogを出力
                using (var mLogger = new EnumRun.Log.MachineLog.MachineLogger(_setting))
                {
                    mLogger.Write();
                }

                //  OldFileをクリア
                DeleteOldFile(_setting.GetLogsPath());
                DeleteOldFile(_setting.GetOutputPath());
            }

            //  SessionLogを出力
            using (var sLogger = new EnumRun.Log.SessionLog.SessionLogger(_setting))
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
            //  必要に応じて実行結果ログを出力させるなどの処理を予定。
        }

        /// <summary>
        /// 保持期間以上前のファイルを削除
        /// </summary>
        /// <param name="targetDirectory"></param>
        public void DeleteOldFile(string targetDirectory)
        {
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
                    _logger.Write(LogLevel.Info, "Old file check => {0}, Delete target count => {1}",
                        targetDirectory, files.Length);
                }
                try
                {
                    foreach (var target in files)
                    {
                        File.Delete(target);
                        _logger.Write(LogLevel.Debug, "Delete => {0}", target);
                    }
                }
                catch
                {
                    _logger.Write(LogLevel.Warn, "Delete failed.");
                }
            }
        }

        #region Serialize/Deserialize

        /// <summary>
        /// セッション管理ファイルを読み込んでデシリアライズ
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private Dictionary<string, EnumRun.Log.SessionLog.LogonSession> DeserializeLastLogonSession(string filePath)
        {
            Dictionary<string, EnumRun.Log.SessionLog.LogonSession> sessions = null;
            try
            {
                using (var sr = new StreamReader(filePath, Encoding.UTF8))
                {
                    sessions =
                        JsonSerializer.Deserialize<Dictionary<string, EnumRun.Log.SessionLog.LogonSession>>(sr.ReadToEnd());
                }
            }
            catch { }
            return sessions ?? new Dictionary<string, EnumRun.Log.SessionLog.LogonSession>();
        }

        /// <summary>
        /// セッション管理ファイルへシリアライズして保存
        /// </summary>
        /// <param name="sessions"></param>
        /// <param name="filePath"></param>
        private void SerializeLogonSession(Dictionary<string, EnumRun.Log.SessionLog.LogonSession> sessions, string filePath)
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
