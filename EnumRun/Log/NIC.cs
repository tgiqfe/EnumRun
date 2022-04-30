using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnumRun.Log
{
    internal class NIC
    {
        public string Name { get; set; }
        public string[] IPv4Addresses { get; set; }
        public string[] IPv4SubnetMasks { get; set; }
        public string[] IPv4DefaultGateways { get; set; }
        public string[] IPv4DNSServers { get; set; }

        //  ここにネットワークインタフェース情報を
    }
}
