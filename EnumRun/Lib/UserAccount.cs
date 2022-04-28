using System.Management;
using System.Security.Principal;

namespace EnumRun.Lib
{
    internal class UserAccount
    {
        private static bool? _isSystemAccount = null;

        private static string[] _systemSIDs = null;

        private static string _currentSID = null;

        /// <summary>
        /// 実行中ユーザーがシステムアカウントかどうか
        /// </summary>
        public static bool IsSystemAccount
        {
            get
            {
                if (_isSystemAccount == null)
                {
                    _systemSIDs = new ManagementClass("Win32_SystemAccount").
                        GetInstances().
                        OfType<ManagementObject>().
                        Select(x => x["SID"] as string).
                        ToArray();
                    _currentSID ??= WindowsIdentity.GetCurrent().User.ToString();
                    _isSystemAccount = _systemSIDs.Contains(_currentSID);
                }
                return (bool)_isSystemAccount;
            }
        }

        /// <summary>
        /// 実行中ユーザーのSIDを取得
        /// </summary>
        public static string CurrentSID
        {
            get
            {
                _currentSID ??= WindowsIdentity.GetCurrent().User.ToString();
                return _currentSID;
            }
        }

        /// <summary>
        /// ドメインユーザーかどうか
        /// </summary>
        public static bool IsDomainUser
        {
            get
            {
                return !UserAccount.IsSystemAccount &&
                    Machine.IsDomain &&
                    (Environment.UserDomainName != Environment.MachineName);
            }
        }

        /// <summary>
        /// 管理者実行しているかどうかの確認
        /// </summary>
        /// <returns></returns>
        public static bool IsRunAdministrator()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            return isAdmin;
        }
    }
}
