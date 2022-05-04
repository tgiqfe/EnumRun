using EnumRun.Lib;
using EnumRun.Log.ProcessLog;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using EnumRun.Log;

namespace EnumRun
{
    internal class Script
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        private ProcessLogger _logger { get; set; }
        public int FileNumber { get; set; }

        public bool Enabled { get; set; }
        public EnumRunOption Option { get; set; }
        private EnumRunSetting _setting { get; set; }
        private Language _language { get; set; }

        private static readonly Regex _pat_fileNum = new Regex(@"^\d+(?=_)");

        public Script() { }
        public Script(string filePath, EnumRunSetting setting, LanguageCollection collection, ProcessLogger logger)
        {
            this.FilePath = filePath;
            this.FileName = Path.GetFileName(filePath);
            this._logger = logger;

            Match match;
            this.FileNumber = (match = _pat_fileNum.Match(FileName)).Success ?
                int.Parse(match.Value) : -1;

            if (setting.Ranges?.Within(this.FileNumber) ?? false)
            {
                this.Enabled = true;
                this.Option = new EnumRunOption(this.FilePath);
                this._setting = setting;
                this._language = collection.GetLanguage(this.FilePath);

                _logger.Write(LogLevel.Info, FileName, "{0} => Enabled", FileName);
                _logger.Write(LogLevel.Debug, FileName, "Language => {0}", _language.ToString());
                _logger.Write(LogLevel.Debug, FileName, "Option => [{0}]", Option.OptionType.ToString());
            }
            else
            {
                _logger.Write(LogLevel.Info, FileName, "{0} => Disabled", FileName);
            }
        }

        /// <summary>
        /// スクリプト実行
        /// </summary>
        /// <returns></returns>
        public Task Process()
        {
            if (CheckStopByOption()) { return Task.Run(() => { }); }

            //  実行前待機
            if (this.Option.Contains(OptionType.BeforeWait) && Option.BeforeTime > 0)
            {
                _logger.Write(LogLevel.Info, FileName, "Before wait, {0}sec", Option.BeforeTime);
                Thread.Sleep(Option.BeforeTime * 1000);
            }

            //  終了待ち/標準出力有りの各パターンの組み合わせ
            //    終了待ち:false/標準出力:false ⇒ wait無し。別プロセスとして非管理で実行
            //    終了待ち:false/標準出力:true  ⇒ スレッド内でのみwait。全スレッド終了待ち
            //    終了待ち:true/標準出力:false  ⇒ スレッド内でもwait。スレッド呼び出し元でもwait
            //    終了待ち:true/標準出力:true   ⇒ スレオッド内でwait。スレッド呼び出し元でもwait
            Task task = null;
            if ((this._setting.DefaultOutput ?? false) || this.Option.Contains(OptionType.Output))
            {
                string outputPath = Path.Combine(
                    this._setting.OutputPath,
                    string.Format("{0}_{1}_{2}.txt",
                        Path.GetFileNameWithoutExtension(this.FilePath),
                        Environment.ProcessId,
                        DateTime.Now.ToString("yyyyMMddHHmmss")));
                _logger.Write(LogLevel.Info, FileName, "Output => {0}", outputPath);
                task = ProcessThreadAndOutput(outputPath);
            }
            else
            {
                task = ProcessThread();
            }
            if (Option.Contains(OptionType.WaitForExit))
            {
                _logger.Write(LogLevel.Info, FileName, "Wait until exit.");
                task.Wait();
            }

            //  実行後待機
            if (this.Option.Contains(OptionType.AfterWait) && Option.AfterTime > 0)
            {
                _logger.Write(LogLevel.Info, FileName, "After wait, {0}sec", Option.AfterTime);
                Thread.Sleep(Option.AfterTime * 1000);
            }

            return task;
        }

        /// <summary>
        /// オプションによって実行対象外と判定するかどうか
        /// </summary>
        /// <returns>実行対象外の場合にtrue</returns>
        private bool CheckStopByOption()
        {
            //  [n]オプション
            //  実行対象外
            if (this.Option.Contains(OptionType.NoRun))
            {
                _logger.Write(LogLevel.Attention, FileName, "Stop script, [n]option.");
                return true;
            }

            //  [a]オプション
            //  ユーザーアカウント制御が「通知しない」場合のみ
            //  Administratorsグループのチェックは行わない。一般ユーザーでも、とりあえずrunasで実行。
            if (this.Option.Contains(OptionType.RunAsAdmin) && !UserAccount.IsDisableUAC())
            {
                _logger.Write(LogLevel.Attention, FileName, "Stop script, [a]option.");
                _logger.Write(LogLevel.Debug, FileName, "Enable User Account Control setting.");
                return true;
            }

            //  [m]オプション
            //  ドメイン参加PCのみ
            if (this.Option.Contains(OptionType.DomainPCOnly) && !Machine.IsDomain)
            {
                _logger.Write(LogLevel.Attention, FileName, "Stop script, [m]option and Workgroup PC.");
                _logger.Write(LogLevel.Debug, FileName, "Workgroup name => {0}", Machine.WorkgroupName);
                return true;
            }

            //  [k]オプション
            //  ワークグループPCのみ
            if (this.Option.Contains(OptionType.WorkgroupPCOnly) && Machine.IsDomain)
            {
                //  未テスト
                _logger.Write(LogLevel.Attention, FileName, "Stop script, [k]option and domain PC.");
                _logger.Write(LogLevel.Debug, FileName, "Domain nam => {0}", Machine.DomainName);
                return true;
            }

            //  [s]オプション
            //  システムアカウントのみ
            if (this.Option.Contains(OptionType.SystemAccountOnly) && !UserAccount.IsSystemAccount)
            {
                _logger.Write(LogLevel.Attention, FileName, "Stop script, [s]option and not system account.");
                _logger.Write(LogLevel.Debug, FileName, "UserName => {0}, SID => {1}", Environment.UserName, UserAccount.CurrentSID);
                return true;
            }

            //  [d]オプション
            //  ドメインユーザーのみ
            if (this.Option.Contains(OptionType.DomainUserOnly) && !UserAccount.IsDomainUser)
            {
                _logger.Write(LogLevel.Attention, FileName, "Stop script, [d]option and local user.");
                _logger.Write(LogLevel.Debug, FileName, "UserName => {0}\\{1}", Environment.UserDomainName, Environment.UserName);
                return true;
            }

            //  [l]オプション
            //  ローカルユーザーのみ
            if (this.Option.Contains(OptionType.LocalUserOnly) && UserAccount.IsDomainUser)
            {
                //  未テスト
                _logger.Write(LogLevel.Attention, FileName, "Stop script, [l]option and domain user.");
                _logger.Write(LogLevel.Debug, FileName, "UserName => {0}\\{1}", Environment.UserDomainName, Environment.UserName);
                return true;
            }

            //  [p]オプション
            //  デフォルトゲートウェイへの通信確認
            if (this.Option.Contains(OptionType.DGReachableOnly) && !Machine.IsReachableDefaultGateway())
            {
                //  未テスト
                _logger.Write(LogLevel.Attention, FileName, "Stop script, [p]option and not reachable to DefaultGateway.");
                _logger.Write(LogLevel.Debug, FileName, "DefaultGateway => {0}", Machine.DefaultGateway);
                return true;
            }

            //  [t]オプション
            //  管理者実行しているかどうか。
            //  管理者として実行させるオプション[a]とは異なるので注意。
            if (this.Option.Contains(OptionType.TrustedOnly) && !UserAccount.IsRunAdministrator())
            {
                _logger.Write(LogLevel.Attention, FileName, "Script stop, [t]option and not Turusteduser");
                _logger.Write(LogLevel.Debug, FileName, "UserName => {0}", Environment.UserName);
                return true;
            }

            return false;
        }

        /// <summary>
        /// プロセス実行
        /// </summary>
        /// <returns></returns>
        private async Task ProcessThread()
        {
            await Task.Run(() =>
            {
                using (Process proc = this._language.GetProcess(this.FilePath, ""))
                {
                    proc.StartInfo.Verb = this.Option.Contains(OptionType.RunAsAdmin) ? "RunAs" : "";
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.Start();
                    if (this.Option.Contains(OptionType.WaitForExit))
                    {
                        proc.WaitForExit();
                    }
                }
            });
        }

        /// <summary>
        /// プロセス実行 (実行結果をファイルに出力)
        /// </summary>
        /// <returns></returns>
        private async Task ProcessThreadAndOutput(string outputPath)
        {
            TargetDirectory.CreateParent(outputPath);

            await Task.Run(() =>
            {
                using (Process proc = this._language.GetProcess(this.FilePath, ""))
                using (StreamWriter sw = new StreamWriter(outputPath, false, new UTF8Encoding(false)))
                {
                    proc.StartInfo.Verb = this.Option.Contains(OptionType.RunAsAdmin) ? "RunAs" : "";
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.RedirectStandardInput = true;
                    proc.OutputDataReceived += (sender, e) => { sw.WriteLine(e.Data); };
                    proc.ErrorDataReceived += (sender, e) => { sw.WriteLine(e.Data); };
                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                    proc.WaitForExit();
                }
            });
        }
    }
}
