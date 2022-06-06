using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScriptDelivery.Logs
{
    internal class LogBodyBase
    {
        /// <summary>
        /// LiteDBに格納時にユニークキーとして使用
        /// </summary>
        [JsonIgnore]
        [LiteDB.BsonId]
        public string Serial { get; set; }

        #region Public parameter

        public virtual string Tag { get { return ""; } }
        public virtual string Date { get; set; }
        public virtual string ProcessName { get; set; }
        public virtual string HostName { get; set; }
        public virtual string UserName { get; set; }

        #endregion

        public virtual string GetJson() { return ""; }

        public virtual Dictionary<string, string> SplitForSyslog() { return null; }

        public virtual string ToConsoleMessage() { return ""; }
    }
}
