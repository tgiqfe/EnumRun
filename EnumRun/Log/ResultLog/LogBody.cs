using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnumRun.Log.ResultLog
{
    internal class LogBody
    {
        public const string TAG = "ResultLog";

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



        #endregion


    }
}
