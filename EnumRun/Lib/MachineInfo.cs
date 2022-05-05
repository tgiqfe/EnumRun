using System;
using System.Text;
using System.Management;
using System.Net.NetworkInformation;

namespace EnumRun.Lib
{
    internal class MachineInfo
    {
        private static bool? _isDomain = null;

        private static string _domainName { get; set; }
        private static string _workgroupName { get; set; }
        private static string _defaultGateway { get; set; }

        /// <summary>
        /// ドメイン参加済みかどうか
        /// </summary>
        public static bool IsDomain
        {
            get
            {
                if (_isDomain == null)
                {
                    var mo = new ManagementClass("Win32_ComputerSystem").
                        GetInstances().
                        OfType<ManagementObject>().
                        FirstOrDefault();
                    _isDomain = (bool)(mo["PartOfDomain"] ?? false);
                    MachineInfo._domainName = mo["Domain"] as string;
                    MachineInfo._workgroupName = mo["Workgroup"] as string;
                }
                return (bool)_isDomain;
            }
        }

        /// <summary>
        /// ドメイン名を取得
        /// </summary>
        public static string DomainName
        {
            get { return MachineInfo.IsDomain ? _domainName : null; }
        }

        /// <summary>
        /// ワークグループ名を取得
        /// </summary>
        public static string WorkgroupName
        {
            get { return MachineInfo.IsDomain ? null : _workgroupName; }
        }

        /// <summary>
        /// デフォルトゲートウェイへの導通可否チェック
        /// もし複数のデフォルトゲートウェイが設定されていた場合は、いずれか1つだけ導通可であればtrue
        /// </summary>
        /// <returns></returns>
        public static bool IsReachableDefaultGateway()
        {
            int maxCount = 4;       //  最大4回チェック
            int interval = 500;     //  インターバル500ミリ秒
            Ping ping = new Ping();
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (GatewayIPAddressInformation gw in nic.GetIPProperties().GatewayAddresses)
                {
                    for (int i = 0; i < maxCount; i++)
                    {
                        PingReply reply = ping.Send(gw.Address);
                        if (reply.Status == IPStatus.Success)
                        {
                            _defaultGateway = gw.Address.ToString();
                            return true;
                        }
                        Thread.Sleep(interval);
                    }
                }
            }
            return false;
        }

        public static string DefaultGateway
        {
            get
            {
                if (_defaultGateway == null)
                {
                    //  もし、IsReachableDefaultGatewayより前に呼び出した場合や、
                    //  DGへ導通不可だった場合は、登録されているDGを全て返す
                    //  (多分DGは1つしか設定しないはずだけど・・・)
                    var list = new List<string>();
                    foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        foreach (GatewayIPAddressInformation gw in nic.GetIPProperties().GatewayAddresses)
                        {
                            list.Add(gw.Address.ToString());
                        }
                    }
                    _defaultGateway = string.Join(", ", list);
                }
                return _defaultGateway ?? "(設定無し)";
            }
        }

    }
}
