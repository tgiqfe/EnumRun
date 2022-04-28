namespace EnumRun.Lib
{
    internal class ExecSessionResult
    {
        public bool Runnable { get; set; }

        public bool _existsFilesPath { get; set; }

        private bool _matchBootupTime { get; set; }
        private bool _matchLogonTime { get; set; }
        private bool _matchLogonId { get; set; }
        private bool _withinExecTime { get; set; }



        //  (案)前回ブート時間/今回ブート時間
        //  (案)前回ログオン時間/今回ログオン時間
        //  (案)前回ログオンID/今回ログオンID
        //  (案)前回実行時間/今回実行時間
        //  (案)システムアカウントの場合はログオン時間の確認不要
        //  (案)settingの設定内容の確認

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
            this.Runnable &= _existsFilesPath;
        }

        public string GetMessage()
        {
            if (this.Runnable)
            {
                return "Runnable:True";
            }
            else if (!_existsFilesPath)
            {
                return "Runnable:False, FilesPath:missing";
            }
            else
            {
                return string.Format("Runnable:False, Bootup:{0}, Logon:{1}, Id:{2}, Exec:{3}",
                    _matchBootupTime,
                    _matchLogonTime,
                    _matchLogonId,
                    _withinExecTime);
            }
        }
    }
}
