using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using EnumRun.Lib;
using EnumRun.Lib.Syslog;
using EnumRun.ScriptDelivery;

namespace EnumRun.Logs.MachineLog
{
    /// <summary>
    /// MachineLog送信用クラス
    /// </summary>
    internal class MachineLogger2 : LoggerBase2<MachineLogBody>
    {
        protected override string _tag { get { return MachineLogBody.TAG; } }

        public MachineLogger2(EnumRunSetting setting, ScriptDeliverySession session)
        {
            Init(_tag, setting, session);
        }

        /// <summary>
        /// ログ出力
        /// </summary>
        public void Write()
        {
            SendAsync(new MachineLogBody()).ConfigureAwait(false);
        }
    }
}
