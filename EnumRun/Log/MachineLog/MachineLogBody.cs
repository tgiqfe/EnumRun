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
    internal class MachineLogBody : LogBodyBase
    {
        public const string TAG = "MachineLog";

        #region Public parameter

        public override string Tag { get { return TAG; } }
        public override string Date { get; set; }
        public override string ProcessName { get; set; }
        public override string HostName { get; set; }
        public override string UserName { get; set; }
        public string OS { get; set; }
        public string OSVersion { get; set; }
        public NicCollection NetworkInterface { get; set; }

        #endregion

        private static int _index = 0;
        private static JsonSerializerOptions _options = null;

        public MachineLogBody() { }
        public MachineLogBody(bool init)
        {
            this.ProcessName = Item.ProcessName;
            this.HostName = Environment.MachineName;
            this.UserName = Environment.UserName;
            this.Serial = $"{Item.Serial}_{_index++}";

            ManagementObject mo = new ManagementClass("Win32_OperatingSystem").
                GetInstances().
                OfType<ManagementObject>().
                First();
            this.OS = mo["Caption"] as string;
            this.OSVersion = mo["Version"] as string;
        }

        public override string GetJson()
        {
            _options ??= GetJsonSerializerOption(
                escapeDoubleQuote: true, false, false, writeIndented: true, convertEnumCamel: true);
            return JsonSerializer.Serialize(this, _options);
        }


    }
}
