using System.Management;
using System.Reflection;
using System.Text;
using System.Text.Json;
using EnumRun.Log.SessionLog;
using EnumRun.Log;

namespace EnumRun.Lib
{
    internal class ExecSession
    {
        #region private classes

        /// <summary>
        /// 確認結果を格納するクラス
        /// </summary>
        public class Result
        {
            public bool Runnable { get; set; } = true;

            public string BootupTime { get; set; }
            public string LogonTime { get; set; }
            public string LogonId { get; set; }
            public string ExecTime { get; set; }

            public string ToLog()
            {
                if (Runnable)
                {
                    return "Runnable => True";
                }
                else
                {
                    var props =
                        this.GetType().GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
                    return "Runnable => False [ " +
                        string.Join(" ", props.
                            Where(x => x.PropertyType == typeof(string)).
                            Select(x => $"{x.Name}={x.GetValue(this)}")) + " ]";
                }
            }
        }


        #endregion

        public bool Enabled { get; set; }

        public ExecSession() { }

        public ExecSession(EnumRunSetting setting, EnumRun.Log.ProcessLog.ProcessLogger logger)
        {
            //  前回セッション
            string filePath = TargetDirectory.GetFile(Item.SESSION_FILE);
            Dictionary<string, LogonSession> lastSessions = DeserializeLastLogonSession(filePath);

            //  今回セッション
            SessionLogBody body = new SessionLogBody();

            //  前回セッションと比較して、実行可否チェック
            StringBuilder sb = new StringBuilder();
            this.Enabled = CheckRunnable(
                lastSessions.ContainsKey(Item.ProcessName) ? lastSessions[Item.ProcessName] : null,
                body.Session,
                sb,
                setting.RestTime ?? 0);
            logger.Write(Enabled ? LogLevel.Info : LogLevel.Warn, null, "Runnable => {0} [{1}]",
                Enabled ? "Enable" : "Disable",
                sb.ToString());

            bool todayFirst = !(lastSessions.Values.
                Where(x => DateTime.Today == x.ExecTime?.Date).
                Any(x => x.TodayFirst ?? false));
            if (todayFirst)
            {
                body.Session.TodayFirst = true;

                //  MachineLogを送信
                using (var mLogger = new EnumRun.Log.MachineLog.MachineLogger(setting))
                {
                    mLogger.Write();
                }

                //  OldFileをクリア
                OldFiles.Clean(setting);
            }

            //  SessionLogを送信
            using (var sLogger = new SessionLogger(setting))
            {
                sLogger.Write(body);
            }

            lastSessions[Item.ProcessName] = body.Session;
            SerializeLogonSession(lastSessions, filePath);
        }

        public void PreProcess() { }

        public void PostProcess() { }

        private bool CheckRunnable(LogonSession lastSession, LogonSession currentSession, StringBuilder sb, int restTime)
        {
            if (lastSession != null)
            {
                bool rest = ((DateTime)currentSession.ExecTime - (DateTime)lastSession.ExecTime).TotalSeconds <= restTime;
                bool bootup = lastSession.BootupTime == currentSession.BootupTime;
                bool logon = lastSession.LogonTime == currentSession.LogonTime;
                bool id = lastSession.LogonId == currentSession.LogonId;

                if (rest)
                {
                    sb.Append("RestTime=Over");
                    sb.Append("]");
                    return true;
                }
                else
                {
                    sb.Append("RestTime=NotOver");
                }

                sb.Append(string.Format("BootupTime={0}, LogonTime={1}, LogonId={2}",
                    bootup ? "SameAsLast" : "Changed",
                    logon ? "SameAsLast" : "Changed",
                    id ? "SameAsLast" : "Changed"));
                return !bootup && !logon && !id;
            }

            return true;
        }






        public static Result PrepareProcess(EnumRunSetting setting, EnumRun.Log.ProcessLog.ProcessLogger logger)
        {
            //  前回セッション
            string filePath = TargetDirectory.GetFile(Item.SESSION_FILE);
            Dictionary<string, LogonSession> lastSessions = DeserializeLastLogonSession(filePath);
            LogonSession lastSession =
                lastSessions.ContainsKey(Item.ProcessName) ? lastSessions[Item.ProcessName] : null;

            //  今回セッション
            SessionLogBody body = new SessionLogBody();
            LogonSession currentSession = body.Session;

            Result ret = new Result();
            if (lastSession != null)
            {
                //  ブート時間(OS起動時間)
                if (lastSession.BootupTime == currentSession.BootupTime)
                {
                    ret.Runnable = false;
                    ret.BootupTime = currentSession.BootupTime?.ToString("yyyy/MM/dd HH:mm:ss");
                }
                else
                {
                    ret.BootupTime = string.Format("{0}~{1}",
                        lastSession.BootupTime?.ToString("yyyy/MM/dd HH:mm:ss"),
                        currentSession.BootupTime?.ToString("yyyy/MM/dd HH:mm:ss"));
                }

                //  ログオン時間
                if (lastSession.LogonTime == currentSession.LogonTime)
                {
                    ret.Runnable = false;
                    ret.LogonTime = currentSession.LogonTime?.ToString("yyyy/MM/dd HH:mm:ss");
                }
                else
                {
                    ret.LogonTime = string.Format("{0}~{1}",
                        lastSession.LogonTime?.ToString("yyyy/MM/dd HH:mm:ss"),
                        currentSession.LogonTime?.ToString("yyyy/MM/dd HH:mm:ss"));
                }

                //  ログオンID
                if (lastSession.LogonId == currentSession.LogonId)
                {
                    ret.Runnable = false;
                    ret.LogonId = currentSession.LogonId;
                }
                else
                {
                    ret.LogonId = string.Format("{0}~{1}", lastSession.LogonId, currentSession.LogonId);
                }

                //  実行時間
                if (((DateTime)currentSession.ExecTime - (DateTime)lastSession.ExecTime).TotalSeconds <= setting.RestTime)
                {
                    ret.Runnable = false;
                    ret.ExecTime = currentSession.ExecTime?.ToString("yyyy/mm/dd hh:mm:ss");
                }
                else
                {
                    ret.Runnable = true;   //  他のチェックが全てfalseでも、前回実行から指定以上の時間が経過していたらtrue
                    ret.ExecTime = string.Format("{0}~{1}",
                        lastSession.ExecTime?.ToString("yyyy/MM/dd HH:mm:ss"),
                        currentSession.ExecTime?.ToString("yyyy/MM/dd HH:mm:ss"));
                }
            }

            lastSessions[Item.ProcessName] = currentSession;
            SerializeLogonSession(lastSessions, filePath);

            return ret;
        }

        /// <summary>
        /// セッション管理ファイルを読み込んでデシリアライズ
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static Dictionary<string, LogonSession> DeserializeLastLogonSession(string filePath)
        {
            Dictionary<string, LogonSession> sessions = null;
            try
            {
                using (var sr = new StreamReader(filePath, Encoding.UTF8))
                {
                    sessions =
                        JsonSerializer.Deserialize<Dictionary<string, LogonSession>>(sr.ReadToEnd());
                }
            }
            catch { }
            return sessions ?? new Dictionary<string, LogonSession>();
        }

        /// <summary>
        /// セッション管理ファイルへシリアライズして保存
        /// </summary>
        /// <param name="sessions"></param>
        /// <param name="filePath"></param>
        private static void SerializeLogonSession(Dictionary<string, LogonSession> sessions, string filePath)
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
    }
}
