using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnumRun.Log.MachineLog
{
    internal class SystemInfoLog
    {
        #region Serial parameter

        /// <summary>
        /// LiteDBに格納時にユニークキーとして使用
        /// </summary>
        [JsonIgnore]
        public string Serial
        {
            get
            {
                if (_seed == null)
                {
                    var md5 = System.Security.Cryptography.MD5.Create();
                    var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(
                        DateTime.Now.Ticks.ToString() + this.GetHashCode().ToString()));
                    _seed = BitConverter.ToString(bytes).Replace("-", "");
                    md5.Clear();
                }
                return $"{_seed}_{_index}";
            }
            set { _seed = value; }
        }
        private string _seed = null;
        private int _index = 0;

        #endregion

        public string Tag { get { return "EnumRun_SystemInfo"; } }
        public string Date { get; set; }
        public string HostName { get; set; }
        public string OS { get; set; }
        public string OSVersion { get; set; }
        public string AppFilePath { get; set; }
        public string AppVersion { get; set; }
        public Nic[] NetworkInterfaces { get; set; }
        public DateTime BootupTime { get; set; }
        public DateTime LogonTime { get; set; }

        private JsonSerializerOptions _options = new JsonSerializerOptions()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        public void Init()
        {
            this.HostName = Environment.MachineName;

            ManagementObject mo = new ManagementClass("Win32_OperatingSystem").
                GetInstances().
                OfType<ManagementObject>().
                First();
            this.OS = mo["Caption"] as string;
            this.OSVersion = mo["Version"] as string;
            this.AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

    }
}
