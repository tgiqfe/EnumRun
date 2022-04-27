using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnumRun.Lib
{
    internal class ExecSessionResult
    {
        public bool Runnable { get; set; }

        private bool _matchBootupTime { get; set; }
        private bool _matchLogonTime { get; set; }
        private bool _matchLogonId { get; set; }
        private bool _withinExecTime { get; set; }

        public ExecSessionResult() { }
        public ExecSessionResult(ExecSession last, ExecSession current, int diff)
        {
            if(last == null)
            {
                this.Runnable = true;
                return;
            }
            this._matchBootupTime = last.LastBootupTime == current.LastBootupTime;
            this._matchLogonTime = last.LastLogonTime == current.LastLogonTime;
            this._matchLogonId = last.LastLogonId == current.LastLogonId;
            this._withinExecTime = last.LastExecTime < current.LastExecTime ?
                ((DateTime)current.LastExecTime - (DateTime)last.LastExecTime).TotalSeconds > diff :
                false;

            this.Runnable = !_matchBootupTime && !_matchLogonTime && !_matchLogonId && !_withinExecTime;
        }

        public string GetMessage()
        {
            return string.Format("Runnable:{0}, Bootup:{1}, Logon:{2}, Id:{3}, Exec:{4}",
                Runnable,
                _matchBootupTime,
                _matchLogonTime,
                _matchLogonId,
                _withinExecTime);
        }
    }
}
