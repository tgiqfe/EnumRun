using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Reflection;
using System.Text.Json;

namespace EnumRun.Log
{
    internal class LogBody
    {
        public string Date { get; set; }
        public LogLevel Level { get; set; }
        public string UserName { get; set; }
        public string HostName { get; set; }
        public string OS { get; set; }
        public string OSVersion { get; set; }
        public string AppVersion { get; set; }
        public string Message { get; set; }

        public void Init()
        {
            this.UserName = Environment.UserName;
            this.HostName = Environment.MachineName;

            ManagementObject mo = new ManagementClass("Win32_OperatingSystem").
                GetInstances().
                OfType<ManagementObject>().
                First();
            this.OS = mo["Caption"] as string;
            this.OSVersion = mo["Version"] as string;
            this.AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public string GetLog(string message)
        {
            return GetLog(LogLevel.Info, message);
        }

        public string GetLog(LogLevel level, string message)
        {
            this.Date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            this.Level = level;
            this.Message = message;

            return JsonSerializer.Serialize(this);
        }
    }
}
