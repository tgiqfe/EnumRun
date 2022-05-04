using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnumRun.Log.SessionLog
{
    internal class LogBody
    {
        public const string TAG = "SessionLog";

        #region Serial parameter

        /// <summary>
        /// LiteDBに格納時にユニークキーとして使用
        /// </summary>
        [JsonIgnore]
        [LiteDB.BsonId]
        public string Serial { get; set; }

        /// <summary>
        /// Serialの通し番号
        /// </summary>
        private static int _index = 0;

        #endregion
        #region Public parameter

        public string Tag { get { return TAG; } }
        public string Date { get; set; }

        public string HostName { get; set; }
        public string UserName { get; set; }
        public string DomainOrWorkgroup { get; set; }
        public string AppName { get; set; }
        public string AppVersion { get; set; }
        public DateTime BootupTime { get; set; }
        public DateTime LogonTime { get; set; }


        #endregion

    }
}
