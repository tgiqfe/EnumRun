using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace EnumRun.Log
{
    internal class NIC
    {
        public string Name { get; set; }
        public string[] IPv4Addresses { get; set; }
        public string[] IPv4SubnetMasks { get; set; }
        public string[] IPv4DefaultGateways { get; set; }
        public string[] IPv4DNSServers { get; set; }
        public string MACAddress { get; set; }
        public NetworkInterfaceType NicType { get; set; }
        public long Speed { get; set; }
        public string DnsSuffix { get; set; }

        public static List<NIC> GetNics()
        {



            return null;
        }

    }
}
