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
            string logTitle = "IsMatch";

            _info ??= new NetworkInfo();

            bool ret = ruleMatch switch
            {
                RuleMatch.Equal => EqualMatch(),
                RuleMatch.Range => RangeMatch(),
                RuleMatch.InNetwork => InNetworkMatch(),
                _ => false,
            };

            _logger.Write(ret ? LogLevel.Debug : LogLevel.Attention,
                logTitle,
                $"MatchType => {ruleMatch}, Match => {ret}");

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
        /// 1～3オクテットは、IPAddress、NetworkAddressのパラメータを参照するか、StartAddress,EndAddressの両方比較後判定
        /// IPAddress, NetworkAddressを参照する場合は、StartAddress,EndAddressは数値型。
        /// StartAddress,EndAddressで1～3オクテットを判定する場合は、IPAddress形式。
        /// </summary>
        /// <returns></returns>
        private bool RangeMatch()
        {
            byte[] targetIP = new byte[4] { 0, 0, 0, 0 };
            int startNum = -1;
            int endNum = -1;
            if (!string.IsNullOrEmpty(IPAddress) && IPAddress.Contains("."))
            {
                //  １～3オクテットを、IPAddressパラメータを使用して判定
                string[] fields = IPAddress.Split('.');
                if (fields.Length >= 4)
                {
                    if (byte.TryParse(fields[0], out byte temp0)) { targetIP[0] = temp0; }
                    if (byte.TryParse(fields[1], out byte temp1)) { targetIP[0] = temp1; }
                    if (byte.TryParse(fields[2], out byte temp2)) { targetIP[0] = temp2; }
                    if (byte.TryParse(fields[3], out byte temp3)) { targetIP[0] = temp3; }    //  パラメータセットの必要はないけれど、一応。
                }
                if (int.TryParse(this.StartAddress, out int tempStart)) { startNum = tempStart; }
                if (int.TryParse(this.EndAddress, out int tempEnd)) { endNum = tempEnd; }
            }
            else if (!string.IsNullOrEmpty(NetworkAddress) && NetworkAddress.Contains("."))
            {
                //  1～3オクテットを、NetworkAddressパラメータを使用して判定
                string[] fields = NetworkAddress.Contains("/") ?
                    NetworkAddress.Substring(0, NetworkAddress.IndexOf(".")).Split(".") :
                    NetworkAddress.Split(".");
                if (fields.Length >= 4)
                {
                    if (byte.TryParse(fields[0], out byte temp0)) { targetIP[0] = temp0; }
                    if (byte.TryParse(fields[1], out byte temp1)) { targetIP[0] = temp1; }
                    if (byte.TryParse(fields[2], out byte temp2)) { targetIP[0] = temp2; }
                    if (byte.TryParse(fields[3], out byte temp3)) { targetIP[0] = temp3; }    //  パラメータセットの必要はないけれど、一応。
                }
                if (int.TryParse(this.StartAddress, out int tempStart)) { startNum = tempStart; }
                if (int.TryParse(this.EndAddress, out int tempEnd)) { endNum = tempEnd; }
            }
            else
            {
                //  1～3オクテットを、StartAddress,EndAddressパラメータを使用して判定
                byte[] startBytes = new byte[4] { 0, 0, 0, 0 };
                byte[] endBytes = new byte[4] { 0, 0, 0, 0 };
                if (StartAddress.Contains("."))
                {
                    string[] fields = StartAddress.Split('.');
                    if (fields.Length >= 4)
                    {
                        if (byte.TryParse(fields[0], out byte temp0)) { startBytes[0] = temp0; }
                        if (byte.TryParse(fields[1], out byte temp1)) { startBytes[0] = temp1; }
                        if (byte.TryParse(fields[2], out byte temp2)) { startBytes[0] = temp2; }
                        if (byte.TryParse(fields[3], out byte temp3)) { startBytes[0] = temp3; }
                    }
                }
                if (EndAddress.Contains("."))
                {
                    string[] fields = EndAddress.Split('.');
                    if (fields.Length >= 4)
                    {
                        if (byte.TryParse(fields[0], out byte temp0)) { endBytes[0] = temp0; }
                        if (byte.TryParse(fields[1], out byte temp1)) { endBytes[0] = temp1; }
                        if (byte.TryParse(fields[2], out byte temp2)) { endBytes[0] = temp2; }
                        if (byte.TryParse(fields[3], out byte temp3)) { endBytes[0] = temp3; }
                    }
                }
                if (startBytes[0] == endBytes[0] && startBytes[1] == endBytes[1] && startBytes[2] == endBytes[2])
                {
                    targetIP[0] = startBytes[0];
                    targetIP[1] = startBytes[1];
                    targetIP[2] = startBytes[2];
                    targetIP[3] = startBytes[3];      //  パラメータセットの必要はないけれど、一応。
                    startNum = startBytes[3];
                    endNum = endBytes[3];
                }
            }

            var _nics = GetNICsFromInterface();

            foreach (var nic in _nics)
            {
                foreach (var addressSet in nic.AddressSets)
                {
                    if (addressSet.IPv4)
                    {
                        byte[] bytes = addressSet.IPAddress.GetAddressBytes();
                        if (bytes[0] == targetIP[0] && bytes[1] == targetIP[1] && bytes[2] == targetIP[2] &&
                            bytes[3] >= startNum && bytes[3] <= endNum)
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
