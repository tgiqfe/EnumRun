using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace EnumRun.ScriptDelivery
{
    internal class SmbSession
    {
        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        private static extern int WNetCancelConnection2(string lpName, int dwFlags, bool fForce);

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        private static extern int WNetAddConnection2(ref NETRESOURCE lpNetResource, string lpPassword, string lpUsername, int dwFlags);

        [StructLayout(LayoutKind.Sequential)]
        public struct NETRESOURCE
        {
            public int dwScope;
            public int dwType;
            public int dwDisplayType;
            public int dwUsage;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpLocalName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpRemoteName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpComment;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpProvider;
        }

        private string _shareName = null;
        private string _userName = null;
        private string _password = null;
        private NETRESOURCE _netResource;

        const int _timeout = 3000;

        public bool Connected { get; private set; }

        public SmbSession(string shareName, string userName, string password)
        {
            this._shareName = shareName;
            this._userName = userName;
            this._password = password;
        }

        public static string GetShareName(string targetPath)
        {
            return targetPath.Substring(2, targetPath.IndexOf("\\", 2) - 2);
        }

        public bool Connect()
        {
            var task = Task.Factory.StartNew(() =>
                WNetAddConnection2(ref _netResource, this._password, this._userName, 0) == 0);
            bool ret = task.Wait(_timeout) && task.Result;
            if (ret)
            {
                this.Connected = true;  //  接続成功
                return true;
            }
            return false;               //  接続失敗
        }

        public void Disconnect()
        {
            if (Connected)
            {
                WNetCancelConnection2(this._shareName, 0, true);
            }
        }
    }
}
