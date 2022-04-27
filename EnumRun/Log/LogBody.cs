using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnumRun.Log
{
    internal class LogBody
    {
        public string Date { get; set; }
        public LogLevel Level { get; set; }
        public string ProcessName { get; set; }
        public string ScriptFile { get; set; }
        public string UserName { get; set; }
        public string HostName { get; set; }
        public string OS { get; set; }
        public string OSVersion { get; set; }
        public string AppVersion { get; set; }
        public string Message { get; set; }

        private JsonSerializerOptions _options = new JsonSerializerOptions()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        public void Init()
        {
            this.ProcessName = Item.ProcessName;
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

        public string GetLog(LogLevel level, string scriptFile, string message)
        {
            this.Date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            this.Level = level;
            this.ScriptFile = scriptFile;
            this.Message = message;

            return JsonSerializer.Serialize(this, _options);
        }
    }
}
