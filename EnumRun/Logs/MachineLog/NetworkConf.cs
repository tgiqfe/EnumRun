using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace EnumRun.Logs.MachineLog
{
    internal class NetworkConf
    {
        public string Name { get; set; }
        public string Adapter { get; set; }
        public string MACAddress { get; set; }
        public string IPAddress { get; set; }
        public string DefaultGateway { get; set; }
        public string DnsServer { get; set; }
        public string DHCPEnabled { get; set; }

        public NetworkConf() { }
        public NetworkConf(bool init) { this.Init(); }

        public void Init()
        {
            var nameList = new List<string>();
            var adapterList = new List<string>();
            var macAddressList = new List<string>();
            var ipAddressList = new List<string>();
            var defaultGatewayList = new List<string>();
            var dnsSeverList = new List<string>();
            var dhcpEnabledList = new List<string>();

            ManagementObject[] mo_adapters =
                new ManagementClass("Win32_NetworkAdapter").
                    GetInstances().
                    OfType<ManagementObject>().
                    Where(x => x["NetConnectionID"] is not null).
                    ToArray();
            ManagementObject[] mo_conves =
                new ManagementClass("Win32_NetworkAdapterConfiguration").
                    GetInstances().
                    OfType<ManagementObject>().
                    ToArray();
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var adapter in mo_adapters)
            {
                string guid = adapter["GUID"] as string;
                ManagementObject conf = mo_conves.
                    FirstOrDefault(x => (x["SettingID"] as string) == guid);
                NetworkInterface ni = nics.FirstOrDefault(x => x.Id == guid);

                nameList.Add((adapter["NetConnectionID"] as string) ?? "null");
                adapterList.Add((adapter["Name"] as string) ?? "null");
                macAddressList.Add((adapter["MACAddress"] as string) ?? "null");

                if (ni == null)
                {
                    ipAddressList.Add("null");
                    defaultGatewayList.Add("null");
                    dnsSeverList.Add("null");
                }
                else
                {
                    var ips = ni.GetIPProperties();
                    string[] addresses = ips.UnicastAddresses.
                        OfType<UnicastIPAddressInformation>().
                        Select(x => string.Format("{0}/{1}",
                            x.Address,
                            x.Address.AddressFamily == AddressFamily.InterNetwork ? x.IPv4Mask : x.PrefixLength)).
                        ToArray();
                    string[] gateways =
                        ips.GatewayAddresses.Select(x => x.Address.ToString()).ToArray();
                    string[] dnssvs = ips.DnsAddresses.Select(x => x.ToString()).ToArray();

                    ipAddressList.Add(addresses?.Length > 0 ?
                        "[" + string.Join(", ", addresses) + "]" : "null");
                    defaultGatewayList.Add(gateways?.Length > 0 ?
                        "[" + string.Join(", ", gateways) + "]" : "null");
                    dnsSeverList.Add(dnssvs?.Length > 0 ?
                        "[" + String.Join(", ", dnssvs) + "]" : "null");
                }

                if (conf == null)
                {
                    dhcpEnabledList.Add("null");
                }
                else
                {
                    bool? dhcpEnabled = conf["DHCPEnabled"] as bool?;
                    dhcpEnabledList.Add(dhcpEnabled == null ?
                        "null" :
                        (bool)dhcpEnabled ? "Enabled" : "Disabled");
                }
            }

            this.Name = string.Join(", ", nameList);
            this.Adapter = string.Join(", ", adapterList);
            this.MACAddress = string.Join(", ", macAddressList);
            this.IPAddress = string.Join(", ", ipAddressList);
            this.DefaultGateway = string.Join(", ", defaultGatewayList);
            this.DnsServer = string.Join(", ", dnsSeverList);
            this.DHCPEnabled = string.Join(", ", dhcpEnabledList);
        }
    }
}
