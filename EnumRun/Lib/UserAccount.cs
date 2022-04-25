using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Security.Principal;

namespace EnumRun.Lib
{
    internal class UserAccount
    {
        private static string[] _systemSIDs = null;

        private static string _currentSID = null;

        /// <summary>
        /// プロセス実行中ユーザーがシステムアカウントかどうかをチェック
        /// </summary>
        /// <returns></returns>
        public static bool IsSystemAccount()
        {
            _systemSIDs ??= new ManagementClass("Win32_SystemAccount").
                GetInstances().
                OfType<ManagementObject>().
                Select(x => x["SID"] as string).
                ToArray();
            _currentSID ??= WindowsIdentity.GetCurrent().User.ToString();

            return _systemSIDs.Contains(_currentSID);
        }
    }
}
