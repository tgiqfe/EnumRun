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
        public DateTime? BootupTime { get; set; }
        public DateTime? LogonTime { get; set; }
        public string LogonId { get; set; }
        public DateTime? ExecTime { get; set; }

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
            this.ProcessName = Item.ProcessName;

            var mo = new ManagementClass("Win32_OperatingSystem").
                GetInstances().
                OfType<ManagementObject>().
                FirstOrDefault();
            if (mo != null)
            {
                this.BootupTime = ManagementDateTimeConverter.ToDateTime(mo["LastBootUpTime"] as string);
            }

            var session = new ManagementClass("Win32_LogonSession").
                GetInstances().
                OfType<ManagementObject>().
                Select(x => new LogonSession(x["StartTime"] as string, x["LogonId"] as string)).
                ToList().
                OrderByDescending(x => x.Time).
                FirstOrDefault();
            this.LogonTime = session?.Time;
            this.LogonId = session?.Id;

            this.ExecTime = DateTime.Now;
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
                lastSessions.ContainsKey(Item.ProcessName) ? lastSessions[Item.ProcessName] : null,
                currentSession,
                setting.RestTime ?? 0);
            result._existsFilesPath = File.Exists(setting.FilesPath);


            lastSessions[Item.ProcessName] = currentSession;
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
