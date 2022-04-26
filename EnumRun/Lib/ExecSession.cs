using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Text.Json;
using System.Text.Json.Serialization;
usign System.IO;

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
                this.Time = DateTime.TryParse(time, out DateTime tempTime) ? tempTime : null;
                this.Id = id;
            }
        }

        public void SetLast()
        {
            this.ProcessName = Item.AssemblyFile;

            var mo = new ManagementClass("Win32_OperatingSystem").
                GetInstances().
                OfType<ManagementObject>().
                FirstOrDefault();
            if (mo != null)
            {
                this.LastBootupTime = DateTime.TryParse(mo["LastBootUpTime"] as string, out DateTime tempTime) ? tempTime : null;
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
        }

        public static void Check()
        {
            ExecSession session = null;

            string sessionFilePath = new string[]
            {
                Path.Combine(Item.WorkDirectory, Item.SESSION_FILE),
                Path.Combine(Item.AssemblyDirectory, Item.SESSION_FILE),
            }.FirstOrDefault(x => File.Exists(x));
            if (sessionFilePath != null)
            {
                using (var sr = new StreamReader(sessionFilePath, Encoding.UTF8))
                {
                    session = JsonSerializer.Deserialize<ExecSession>(sr.ReadToEnd());
                }
            }
            if (session == null)
            {
                session = new ExecSession();
                session.SetLast();
            }


            //  ブートセッション管理用
            //  途中！






        }



    }
}
