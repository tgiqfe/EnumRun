using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnumRun.Log.SessionLog
{
    internal class SessionLogBody : LogBodyBase
    {
        public const string TAG = "SessionLog";

        #region Public parameter

        public override string Tag { get { return TAG; } }
        public override string Date { get; set; }
        public override string ProcessName { get; set; }
        public override string HostName { get; set; }

        public override string UserName { get; set; }
        public string UserDomain { get; set; }
        public bool? IsSystemAccount { get; set; }
        public string LogonId { get; set; }

        public string AppName { get; set; }
        public string AppVersion { get; set; }
        public string ExecutePath { get; set; }
        public DateTime? BootupTime { get; set; }
        public DateTime? LogonTime { get; set; }
        public DateTime? ExecTime { get; set; }

        #endregion

        private static int _index = 0;
        private static JsonSerializerOptions _options = null;

        public SessionLogBody() { }
        public SessionLogBody(bool init)
        {
            this.ProcessName = Item.ProcessName;
            this.HostName = Environment.MachineName;
            this.UserName = Environment.UserName;
            this.Serial = $"{Item.Serial}_{_index++}";





        }

    }
}
