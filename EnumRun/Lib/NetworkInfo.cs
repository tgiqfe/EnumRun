using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Management;
using System.Net;
using System.Text.RegularExpressions;

namespace EnumRun.Lib
{
    internal class NetworkInfo
    {
        #region Private class

        public class NIC
        {
            public string Name { get; set; }
            public string GUID { get; set; }
            public IEnumerable<AddressSet> AddressSets { get; set; }

            private string[] _ipAddresses = null;
            public string[] GetIPAddresses()
            {
                _ipAddresses ??= this.AddressSets.Select(x => x.IPAddress.ToString()).ToArray();
                return _ipAddresses;
            }
        }

        public class AddressSet
        {
            public IPAddress IPAddress { get; set; }
            public IPAddress SunbnetMask { get; set; }
            public bool IPv4 { get; set; }
            public int PrefixLength { get; set; }
        }

        #endregion

        public List<NIC> NICs { get; set; }

        public NetworkInfo()
        {
            this.NICs = new List<NIC>();

            var adapters = new ManagementClass("Win32_NetworkAdapter").
                GetInstances().
                OfType<ManagementObject>().
                Where(x => x["NetConnectionID"] != null);

            var nis = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var adapter in adapters)
            {
                string guid = adapter["GUID"] as string;
                var ni = nis.FirstOrDefault(x => x.Id == guid);

                var addressSets = ni == null ? null :
                    ni.GetIPProperties().UnicastAddresses.
                        Select(x => new AddressSet()
                        {
                            IPAddress = x.Address,
                            SunbnetMask = x.IPv4Mask,
                            IPv4 = x.Address.AddressFamily == AddressFamily.InterNetwork,
                            PrefixLength = x.PrefixLength,
                        });
                NICs.Add(new NIC()
                {
                    Name = adapter["NetConnectionID"] as string,
                    GUID = guid,
                    AddressSets = addressSets
                });
            }
        }

        public static AddressSet GetAddressSet(string text)
        {
            var addressSet = new AddressSet() { IPv4 = true, };

            string textPre = text;
            string textSuf = "24";
            if (textPre.Contains("/"))
            {
                textSuf = textPre.Substring(textPre.IndexOf("/") + 1);
                textPre = textPre.Substring(0, textPre.IndexOf("/"));
            }
            if (int.TryParse(textSuf, out int tempInt))
            {
                string bits = new string('1', tempInt).PadRight(32, '0');
                addressSet.PrefixLength = tempInt;
                addressSet.SunbnetMask = new IPAddress(
                    new byte[4] {
                        Convert.ToByte(bits.Substring(0, 8)),
                        Convert.ToByte(bits.Substring(8, 8)),
                        Convert.ToByte(bits.Substring(16, 8)),
                        Convert.ToByte(bits.Substring(24, 8))
                    });
            }
            else if (IPAddress.TryParse(textSuf, out IPAddress subnetMask))
            {
                addressSet.SunbnetMask = subnetMask;
                byte[] bytes = subnetMask.GetAddressBytes();
                string bits = string.Format("{0}{1}{2}{3}",
                    Convert.ToString(bytes[0], 2),
                    Convert.ToString(bytes[1], 2),
                    Convert.ToString(bytes[2], 2),
                    Convert.ToString(bytes[3], 2));
                addressSet.PrefixLength = bits.Length - bits.Replace("1", "").Length;
            }

            addressSet.IPAddress = IPAddress.TryParse(textPre, out IPAddress ipAddress) ?
                ipAddress :
                null;

            return addressSet;
        }
    }
}
