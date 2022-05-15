using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptDelivery.Lib.Syslog
{
    /// <summary>
    /// RFC5424用 構造化データ(Strctured-Data)
    /// </summary>
    internal class StructuredData
    {
        public const int DefaultPrivateEnterpriseNumber = 32473;

        public string SdId = null;
        public Dictionary<string, string> SdParam = null;

        public StructuredData() { }

        public StructuredData(string sdId, Dictionary<string, string> sdParam)
        {
            this.SdId = sdId.Contains("@") ? sdId : $"{sdId}@{DefaultPrivateEnterpriseNumber}";
            this.SdParam = sdParam;
        }
    }
}
