using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumRun.Lib;
using ScriptDelivery.Maps;
using System.Net.Sockets;
using System.Net;
using ScriptDelivery.Maps.Requires;
using ScriptDelivery;
using EnumRun.Logs;
using EnumRun.Lib.Infos;

namespace EnumRun.ScriptDelivery.Maps.Matcher
{
    /// <summary>
    /// IPアドレスのマッチ確認
    /// - Equal     : 完全一致確認。ワイルドカードで部分確認にも対応
    /// - Range     : IPv4アドレスで、第四オクテットのみ範囲確認
    /// - InNetwork : IPv4アドレスで、指定のネットワークアドレスに所属しているかどうかの確認
    /// </summary>
    internal class IPAddressMatcher : MatcherBase
    {
        [MatcherParameter, Keys("IPAddress")]
        public string IPAddress { get; set; }

        [MatcherParameter, Keys("NetworkAddress")]
        public string NetworkAddress { get; set; }

        [MatcherParameter, Keys("Start")]
        public string StartAddress { get; set; }

        [MatcherParameter, Keys("End")]
        public string EndAddress { get; set; }

        [MatcherParameter, Keys("Interface")]
        public string Interface { get; set; }

        private static NetworkInfo _info = null;

        public override bool IsMatch(RuleMatch ruleMatch)
        {
            _info ??= new NetworkInfo();

            bool ret = ruleMatch switch
            {
                RuleMatch.Equal => EqualMatch(),
                RuleMatch.Range => RangeMatch(),
                RuleMatch.InNetwork => InNetworkMatch(),
                _ => false,
            };

            _logger.Write(ret ? LogLevel.Debug : LogLevel.Attention, $"MatchType => {ruleMatch}, Match => {ret}");

            return ret;
        }

        #region Match methods

        /// <summary>
        /// Equal確認
        /// </summary>
        /// <returns></returns>
        private bool EqualMatch()
        {
            var _nics = GetNICsFromInterface();

            if (IPAddress.Contains("*"))
            {
                var pattern = this.IPAddress.GetWildcardPattern();
                foreach (var nic in _nics)
                {
                    bool ret = nic.GetIPAddresses().Any(y => pattern.IsMatch(y));
                    if (ret) { return true; }
                }
            }
            else
            {
                foreach (var nic in _nics)
                {
                    bool ret = nic.GetIPAddresses().Any(y => y == IPAddress);
                    if (ret) { return true; }
                }
            }
            return false;
        }

        /// <summary>
        /// 第四オクテットの範囲確認
        /// </summary>
        /// <returns></returns>
        private bool RangeMatch()
        {
            //  [実装予定] this.IPAddressの1～3オクテットの値とはマッチ確認必要有り

            var _nics = GetNICsFromInterface();

            int startNum = int.TryParse(this.StartAddress, out int tempStart) ? tempStart : 0;
            int endNum = int.TryParse(this.EndAddress, out int tempEnd) ? tempEnd : 0;

            foreach (var nic in _nics)
            {
                foreach (var addressSet in nic.AddressSets)
                {
                    if (addressSet.IPv4)
                    {
                        byte[] bytes = addressSet.IPAddress.GetAddressBytes();
                        if (bytes[3] >= startNum && bytes[3] <= endNum)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// ネットワーク所属確認
        /// </summary>
        /// <returns></returns>
        private bool InNetworkMatch()
        {
            var _nics = GetNICsFromInterface();

            var nwAddressSet = NetworkInfo.GetAddressSet(this.NetworkAddress);
            byte[] subnetMaskBytes = nwAddressSet.SunbnetMask.GetAddressBytes();

            foreach (var nic in _nics)
            {
                foreach (var addressSet in nic.AddressSets)
                {
                    if (addressSet.IPv4)
                    {
                        byte[] ipAddressBytes = addressSet.IPAddress.GetAddressBytes();
                        var tempAddress = new IPAddress(new byte[4]
                        {
                            (byte)(ipAddressBytes[0] & subnetMaskBytes[0]),
                            (byte)(ipAddressBytes[1] & subnetMaskBytes[1]),
                            (byte)(ipAddressBytes[2] & subnetMaskBytes[2]),
                            (byte)(ipAddressBytes[3] & subnetMaskBytes[3])
                        });
                        if (tempAddress.Equals(nwAddressSet.IPAddress))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        #endregion

        private IEnumerable<NetworkInfo.NIC> GetNICsFromInterface()
        {
            if (!string.IsNullOrEmpty(this.Interface))
            {
                if (Interface.Contains("*"))
                {
                    var pattern = Interface.GetWildcardPattern();
                    return _info.NICs.Where(x => pattern.IsMatch(x.Name));
                }
                else
                {
                    return _info.NICs.Where(x => x.Name.Equals(Interface, StringComparison.OrdinalIgnoreCase));
                }
            }
            return _info.NICs;
        }
    }
}
