using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace EnumRun.Lib
{
    internal class ExecSession
    {
        public string ProcessName { get; set; }
        public DateTime? LastBootupTime { get; set; }
        public DateTime? LastLogonTime { get; set; }
        public string LastLogonId { get; set; }
        public DateTime? LastExecTime { get; set; }

        private class LogonSession
        {
            public DateTime? Time { get; set; }
            public string Id { get; set; }
            public LogonSession(string time, string id)
            {
                this.Time = ManagementDateTimeConverter.ToDateTime(time as string);
                this.Id = id;
            }
        }

        public void SetLast()
        {
            this.ProcessName = Item.ExecFileName;

            var mo = new ManagementClass("Win32_OperatingSystem").
                GetInstances().
                OfType<ManagementObject>().
                FirstOrDefault();
            if (mo != null)
            {
                this.LastBootupTime = ManagementDateTimeConverter.ToDateTime(mo["LastBootUpTime"] as string);
            }

            var session = new ManagementClass("Win32_LogonSession").
                GetInstances().
                OfType<ManagementObject>().
                Select(x => new LogonSession(x["StartTime"] as string, x["LogonId"] as string)).
                ToList().
                OrderByDescending(x => x.Time).
                FirstOrDefault();
            this.LastLogonTime = session?.Time;
            this.LastLogonId = session?.Id;

            this.LastExecTime = DateTime.Now;
        }

        /// <summary>
        /// 前回セッションの情報を参照し、今回セッションの実行可否を確認
        /// ※後でもう少し整理する予定
        /// </summary>
        /// <param name="setting"></param>
        /// <returns>trueの場合、実行可能</returns>
        public static ExecSessionResult Check(EnumRunSetting setting)
        {
            Dictionary<string, ExecSession> lastSessions = null;
            string sessionFilePath = new string[]
            {
                Path.Combine(Item.WorkDirectory, Item.SESSION_FILE),
                Path.Combine(Item.ExecDirectoryPath, Item.SESSION_FILE),
            }.FirstOrDefault(x => File.Exists(x));
            if (sessionFilePath != null)
            {
                try
                {
                    using (var sr = new StreamReader(sessionFilePath, Encoding.UTF8))
                    {
                        lastSessions = JsonSerializer.Deserialize<Dictionary<string, ExecSession>>(sr.ReadToEnd());
                    }
                }
                catch { }
            }
            if (lastSessions == null)
            {
                lastSessions = new Dictionary<string, ExecSession>();
            }

            ExecSession currentSession = new ExecSession();
            currentSession.SetLast();

            var result = new ExecSessionResult(
                lastSessions.ContainsKey(Item.ExecFileName) ? lastSessions[Item.ExecFileName] : null,
                currentSession,
                setting.RestTime);

            lastSessions[Item.ExecFileName] = currentSession;
            sessionFilePath ??= Path.Combine(Item.WorkDirectory, Item.SESSION_FILE);
            ParentDirectory.Create(sessionFilePath);
            try
            {
                using (var sw = new StreamWriter(sessionFilePath, false, Encoding.UTF8))
                {
                    string json = JsonSerializer.Serialize(lastSessions,
                        new JsonSerializerOptions() { WriteIndented = true });
                    sw.WriteLine(json);
                }
            }
            catch { }

            return result;
        }



    }
}
