using System.Management;
using System.Net.NetworkInformation;

namespace EnumRun.Lib
{
    internal class Machine
    {
        private static bool? _isDomain = null;

        private static string _domainOrWorkgroupName { get; set; }

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
                        OfType<ManagementClass>().
                        FirstOrDefault();
                    _isDomain = (bool)mo["PartOfDomain"];
                    _domainOrWorkgroupName = mo["Domain"] as string;
                }
                return (bool)_isDomain;
            }
        }

        /// <summary>
        /// ドメイン名を取得
        /// </summary>
        public static string DomainName
        {
            get { return Machine.IsDomain ? _domainOrWorkgroupName : null; }
        }

        /// <summary>
        /// ワークグループ名を取得
        /// </summary>
        public static string WorkgroupName
        {
            get { return Machine.IsDomain ? null : _domainOrWorkgroupName; }
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
                            return true;
                        }
                        Thread.Sleep(interval);
                    }
                }
            }
            return false;
        }
    }
}
