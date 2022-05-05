using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using EnumRun.Lib;

namespace EnumRun.Log.SessionLog
{
    /// <summary>
    /// ログオンセッション情報の保存用クラス
    /// </summary>
    internal class LogonSession
    {
        public DateTime? ExecTime { get; set; }
        public DateTime? BootupTime { get; set; }
        public DateTime? LogonTime { get; set; }
        public string LogonId { get; set; }
        public bool? TodayFirst { get; set; }

        public static Dictionary<string, LogonSession> Deserialize()
        {
            Dictionary<string, LogonSession> sessions = null;
            string filePath = TargetDirectory.GetFile(Item.SESSION_FILE);
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

        public static void Serialize(Dictionary<string, LogonSession> sessions)
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
}
