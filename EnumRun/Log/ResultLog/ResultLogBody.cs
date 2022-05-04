using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnumRun.Log.ResultLog
{
    internal class ResultLogBody : LogBodyBase
    {
        public const string TAG = "ResultLog";

        #region Public parameter

        public override string Tag { get { return TAG; } }
        public override string Date { get; set; }
        public override string ProcessName { get; set; }
        public override string HostName { get; set; }
        public override string UserName { get; set; }

        #endregion

        private static int _index = 0;
        private static JsonSerializerOptions _options = null;

    }
}
