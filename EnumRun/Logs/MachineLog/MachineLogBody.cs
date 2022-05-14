using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnumRun.Lib;

namespace EnumRun.Logs.MachineLog
{
    /// <summary>
    /// 実行中のPC自体の情報を格納
    /// </summary>
    internal class MachineLogBody : LogBodyBase
    {
        public const string TAG = "MachineLog";

        #region Public parameter

        public override string Tag { get { return TAG; } }
        public override string Date { get; set; }
        public override string ProcessName { get; set; }
        public override string HostName { get; set; }
        public override string UserName { get; set; }
        public string DomainName { get; set; }
        public string OS { get; set; }
        public string OSVersion { get; set; }
        public NetworkConf Network { get; set; }

        #endregion

        private static int _index = 0;
        private static JsonSerializerOptions _options = null;

        public MachineLogBody()
        {
            this.ProcessName = Item.ProcessName;
            this.HostName = Environment.MachineName;
            this.UserName = Environment.UserName;
            this.Serial = $"{Item.Serial}_{_index++}";

            this.Date = DateTime.Now.ToString("yyyy/MM:dd HH:mm:ss");
            ManagementObject mo = new ManagementClass("Win32_OperatingSystem").
                GetInstances().
                OfType<ManagementObject>().
                First();

            this.DomainName = MachineInfo.IsDomain ? MachineInfo.DomainName : MachineInfo.WorkgroupName;
            this.OS = mo["Caption"] as string;
            this.OSVersion = mo["Version"] as string;
            this.Network = new NetworkConf(init: true);
        }

        public override string GetJson()
        {
            _options ??= GetJsonSerializerOption(
                escapeDoubleQuote: true,
                false,
                false,
                writeIndented: true,
                convertEnumCamel: true);
            return JsonSerializer.Serialize(this, _options);
        }

        public Dictionary<string, string> GetSyslogMessage()
        {
            var ret = new Dictionary<string, string>();
            ret["MachineInfo"] =
                string.Format("ProcessName => {0}, HostName => {1}, UserName => {2}, DomainName => {3}, OS => {4}, OSVersion => {5}",
                    this.ProcessName, this.HostName, this.UserName, this.DomainName, this.OS, this.OSVersion);
            ret["Network_Name"] = Network.Name;
            ret["Network_Adapter"] = Network.Adapter;
            ret["Network_MACAddres"] = Network.MACAddress;
            ret["Network_IPAddress"] = Network.IPAddress;
            ret["Network_DefaultGateway"] = Network.DefaultGateway;
            ret["Network_DnsServer"] = Network.DnsServer;
            ret["Network_DHCPEnabled"] = Network.DHCPEnabled;

            return ret;
        }
    }
}
