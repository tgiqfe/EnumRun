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
    internal class LogBody
    {
        public const string TAG = "MachineLog";

        #region Serial parameter

        /// <summary>
        /// LiteDBに格納時にユニークキーとして使用
        /// </summary>
        [JsonIgnore]
        [LiteDB.BsonId]
        public string Serial { get; set; }

        /// <summary>
        /// Serialの通し番号
        /// </summary>
        private static int _index = 0;

        #endregion
        #region Public parameter

        public string Tag { get { return TAG; } }
        public string Date { get; set; }
        public string HostName { get; set; }
        public string OS { get; set; }
        public string OSVersion { get; set; }
        public NicCollection NetworkInterface { get; set; }
        
        #endregion

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
            


        }

    }
}
