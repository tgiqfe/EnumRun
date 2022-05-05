using System.Management;
using System.Security.Principal;
using Microsoft.Win32;

namespace EnumRun.Lib
{
    internal class UserInfo
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
                return !UserInfo.IsSystemAccount &&
                    MachineInfo.IsDomain &&
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

        /// <summary>
        /// ユーザーアカウント制御が「通知しない」に設定されているかどうか
        /// レジストリ値から判断。
        /// </summary>
        /// <returns></returns>
        public static bool IsDisableUAC()
        {
            using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System"))
            {
                int? consentPromptBehaviorAdmin = regKey.GetValue("ConsentPromptBehaviorAdmin", null) as int?;
                int? promptOnSecureDesktop = regKey.GetValue("PromptOnSecureDesktop", null) as int?;
                if (consentPromptBehaviorAdmin == 0 && promptOnSecureDesktop == 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
