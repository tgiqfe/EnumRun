using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Management;

namespace EnumRun.Log.MachineLog
{
    internal class Nic
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public NetworkInterfaceType NicType { get; set; }
        public long Speed { get; set; }
        public string MACAddress { get; set; }
        public string[] IPAddresses { get; set; }
        public string[] PrefixLength { get; set; }
        public string[] DefaultDGateways { get; set; }
        public string[] DNSServers { get; set; }
        public string DnsSuffix { get; set; }

        public bool DHCPEnabled { get; set; }
        public string DHCPServer { get; set; }
        public string AdapterName { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Nic() { }

        /// <summary>
        /// 引数付きコンストラクタ。パラメータも同時にセット
        /// </summary>
        /// <param name="ni"></param>
        /// <param name="mo_confs"></param>
        /// <param name="mo_adapters"></param>
        public Nic(NetworkInterface ni, IEnumerable<ManagementObject> mo_confs, IEnumerable<ManagementObject> mo_adapters)
        {
            this.Name = ni.Name;
            this.Description = ni.Description;
            this.NicType = ni.NetworkInterfaceType;
            this.Speed = ni.Speed;

            var mac = ni.GetPhysicalAddress().GetAddressBytes();
            this.MACAddress = string.Format("{0:x2}:{1:x2}:{2:x2}:{3:x2}:{4:x2}:{5:x2}",
                mac[0], mac[1], mac[2], mac[3], mac[4], mac[5]);

            var ips = ni.GetIPProperties();
            this.IPAddresses =
                ips.UnicastAddresses.OfType<UnicastIPAddressInformation>().Select(x => x.Address.ToString()).ToArray();
            this.PrefixLength =
                ips.UnicastAddresses.OfType<UnicastIPAddressInformation>().Select(x => x.PrefixLength.ToString()).ToArray();
            this.DefaultDGateways = ips.GatewayAddresses.Select(x => x.Address.ToString()).ToArray();

            this.DNSServers = ips.DnsAddresses.Select(x => x.ToString()).ToArray();
            this.DnsSuffix = ips.DnsSuffix;

            var mo_conf = mo_confs.FirstOrDefault(x => x["SettingID"] as string == ni.Id);
            this.DHCPServer = mo_conf["DHCPServer"] as string;
            this.DHCPEnabled = (bool)mo_conf["DHCPEnabled"];

            var mo_adapter = mo_adapters.FirstOrDefault(x => x["GUID"] as string == ni.Id);
            this.AdapterName = mo_adapter["Name"] as string;
        }

        public static List<Nic> GetNicCollection()
        {
            var list = new List<Nic>();
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces().Where(x =>
               x.OperationalStatus == OperationalStatus.Up &&
               x.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
               x.NetworkInterfaceType != NetworkInterfaceType.Loopback))
            {
                list.Add(new Nic(
                    ni,
                    new ManagementClass("Win32_NetworkAdapterConfiguration").
                        GetInstances().
                        OfType<ManagementObject>(),
                    new ManagementClass("Win32_NetworkAdapter").
                        GetInstances().
                        OfType<ManagementObject>()));
            }
            return list;
        }
    }
}
