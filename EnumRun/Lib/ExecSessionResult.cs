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

        //  (案)前回ブート時間/今回ブート時間
        //  (案)前回ログオン時間/今回ログオン時間
        //  (案)前回ログオンID/今回ログオンID
        //  (案)前回実行時間/今回実行時間
        //  (案)システムアカウントの場合はログオン時間の確認不要

        public ExecSessionResult() { }
        public ExecSessionResult(ExecSession last, ExecSession current, int diff)
        {
            if (last == null)
            {
                this.Runnable = true;
                return;
            }
            this._matchBootupTime = last.BootupTime == current.BootupTime;
            this._matchLogonTime = last.LogonTime == current.LogonTime;
            this._matchLogonId = last.LogonId == current.LogonId;
            this._withinExecTime = last.ExecTime < current.ExecTime ?
                ((DateTime)current.ExecTime - (DateTime)last.ExecTime).TotalSeconds <= diff :
                false;

            this.Runnable = !_withinExecTime ||
                (!_matchBootupTime && !_matchLogonTime && !_matchLogonId);
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
