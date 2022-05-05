using System.Management;
using System.Reflection;
using System.Text;
using System.Text.Json;
using EnumRun.Log.SessionLog;

namespace EnumRun.Lib
{
    internal class ExecSession
    {
        #region private classes
     
        /*
        /// <summary>
        /// ログオンセッション情報の保存用クラス
        /// </summary>
        public class Session
        {
            public DateTime? BootupTime { get; set; }
            public DateTime? LogonTime { get; set; }
            public string LogonId { get; set; }
            public DateTime? ExecTime { get; set; }
            
            public bool? MachineLogSend { get; set; }
            public bool? OldFileCleaned { get; set; }

            public static Dictionary<string, Session> Deserialize()
            {
                Dictionary<string, Session> sessions = null;
                string filePath = TargetDirectory.GetFile(Item.SESSION_FILE);
                try
                {
                    using (var sr = new StreamReader(filePath, Encoding.UTF8))
                    {
                        sessions =
                            JsonSerializer.Deserialize<Dictionary<string, Session>>(sr.ReadToEnd());
                    }
                }
                catch { }
                return sessions ?? new Dictionary<string, Session>();
            }

            public static void Serialize(Dictionary<string, Session> sessions)
            {
                string filePath = TargetDirectory.GetFile(Item.SESSION_FILE);
                using (var sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    string json = JsonSerializer.Serialize(
                        sessions,
                        new JsonSerializerOptions() { WriteIndented = true });
                    sw.WriteLine(json);
                }
            }
        }

        /// <summary>
        /// ログオン情報確認用クラス
        /// </summary>
        private class LogonInfo
        {
            public DateTime? Time { get; set; }
            public string Id { get; set; }
            public LogonInfo(string time, string id)
            {
                this.Time = ManagementDateTimeConverter.ToDateTime(time as string);
                this.Id = id;
            }
        }
        */

        /// <summary>
        /// 確認結果を格納するクラス
        /// </summary>
        public class Result
        {
            public bool Runnable { get; set; } = true;

            public string BootupTime { get; set; }
            public string LogonTime { get; set; }
            public string LogonId { get; set; }
            public string FilesPath { get; set; }
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




        public static Result Check(EnumRunSetting setting)
        {

            //  前回セッション
            string filePath = TargetDirectory.GetFile(Item.SESSION_FILE);
            //Dictionary<string, LogonSession> lastSessions = LogonSession.Deserialize();
            Dictionary<string, LogonSession> lastSessions = DeserializeLastLogonSession(filePath);
            LogonSession lastSession =
                lastSessions.ContainsKey(Item.ProcessName) ? lastSessions[Item.ProcessName] : null;

            //  今回セッション
            /*
            var logonInfo = new ManagementClass("Win32_LogonSession").
                GetInstances().
                OfType<ManagementObject>().
                Select(x => new LogonInfo(x["StartTime"] as string, x["LogonId"] as string)).
                ToList().
                OrderByDescending(x => x.Time).
                FirstOrDefault();
            Session currentSession = new Session()
            {
                BootupTime = ManagementDateTimeConverter.ToDateTime(
                    new ManagementClass("Win32_OperatingSystem").
                        GetInstances().
                        OfType<ManagementObject>().
                        FirstOrDefault()?["LastBootUpTime"] as string),
                LogonTime = logonInfo?.Time,
                LogonId = logonInfo?.Id,
                ExecTime = DateTime.Now
            };
            */
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
            //LogonSession.Serialize(lastSessions);
            SerializeLogonSession(lastSessions, filePath);

            return ret;
        }

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
