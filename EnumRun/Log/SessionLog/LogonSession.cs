using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnumRun.Log.SessionLog
{
    /// <summary>
    /// ログオンセッション情報の保存用クラス
    /// </summary>
    internal class LogonSession
    {
        public DateTime? BootupTime { get; set; }
        public DateTime? LogonTime { get; set; }
        public string LogonId { get; set; }
        public DateTime? ExecTime { get; set; }
    }
}
