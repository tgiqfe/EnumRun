using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumRun.Lib;
using LiteDB;
using EnumRun.Logs;
using EnumRun.Lib.Syslog;
using System.Diagnostics;
using EnumRun.ScriptDelivery;

namespace EnumRun.Logs.SessionLog
{
    /// <summary>
    /// SessionLog送信用クラス
    /// </summary>
    internal class SessionLogger2 : LoggerBase2<SessionLogBody>
    {
        protected override bool _logAppend { get { return true; } }
        protected override string _tag { get { return SessionLogBody.TAG; } }

        public SessionLogger2(EnumRunSetting setting, ScriptDeliverySession session)
        {
            Init(_tag, setting, session);
        }

        public void Write(SessionLogBody body)
        {
            SendAsync(body).ConfigureAwait(false);
        }
    }
}
