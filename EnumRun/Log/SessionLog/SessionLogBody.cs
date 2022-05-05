using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnumRun.Lib;
using System.Management;
using System.IO;
using System.Reflection;

namespace EnumRun.Log.SessionLog
{
    internal class SessionLogBody : LogBodyBase
    {
        public const string TAG = "SessionLog";

        #region Private class

        /// <summary>
        /// ログオン情報確認用クラス
        /// </summary>
        private class LogonInfo
        {
            public DateTime? Time { get; set; }
            public string Id { get; set; }
            public LogonInfo(string time, string id)
            {
                this.Time = ManagementDateTimeConverter.ToDateTime(time as string);
                this.Id = id;
            }
        }

        #endregion

        #region Public parameter

        public override string Tag { get { return TAG; } }
        public override string Date { get; set; }
        public override string ProcessName { get; set; }
        public override string HostName { get; set; }

        public override string UserName { get; set; }
        public string UserDomain { get; set; }
        public bool? SystemAccount { get; set; }

        public string AppPath { get; set; }
        public string AppVersion { get; set; }
        public LogonSession Session { get; set; }

        #endregion

        private static int _index = 0;
        private static JsonSerializerOptions _options = null;

        public SessionLogBody() { }
        public SessionLogBody(bool init) { Init(); }

        public void Init()
        {
            this.ProcessName = Item.ProcessName;
            this.HostName = Environment.MachineName;
            this.UserName = Environment.UserName;
            this.Serial = $"{Item.Serial}_{_index++}";

            this.UserDomain = Environment.UserDomainName;
            this.SystemAccount = UserInfo.IsSystemAccount;
            this.AppPath = Item.ExecFilePath;
            this.AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            LogonInfo logonInfo = new ManagementClass("Win32_LogonSession").
                GetInstances().
                OfType<ManagementObject>().
                Select(x => new LogonInfo(x["StartTime"] as string, x["LogonId"] as string)).
                ToList().
                OrderByDescending(x => x.Time).
                FirstOrDefault();
            this.Session = new LogonSession()
            {
                BootupTime = ManagementDateTimeConverter.ToDateTime(
                    new ManagementClass("Win32_OperatingSystem").
                        GetInstances().
                        OfType<ManagementObject>().
                        FirstOrDefault()?["LastBootUpTime"] as string),
                LogonTime = logonInfo?.Time,
                LogonId = logonInfo?.Id,
                ExecTime = DateTime.Now
            };
        }

        public override string GetJson()
        {
            _options ??= GetJsonSerializerOption(
                false, 
                false, 
                false, 
                writeIndented: true,
                false);
            return JsonSerializer.Serialize(this, _options);
        }

        public Dictionary<string, string> GetSyslogMessage()
        {
            var ret = new Dictionary<string, string>();
            ret["UserInfo"] =
                string.Format("ProcessName => {0}, HostName => {1}, UserName => {2}, UserDomain => {3}, SystemAccount => {4}",
                    this.ProcessName, this.HostName, this.UserName, this.UserDomain, this.SystemAccount);
            ret["AppInfo"] =
                string.Format("AppPath => {0}, AppVersion => {1}",
                    this.AppPath, this.AppVersion);
            ret["LogonSession"] =
                string.Format("BootupTime => {0}, LogonTime => {1}, LogonId => {2}, ExecTime => {3}",
                    this.Session.BootupTime, this.Session.LogonTime, this.Session.LogonId, this.Session.ExecTime);
            
            return ret;
        }
    }
}
