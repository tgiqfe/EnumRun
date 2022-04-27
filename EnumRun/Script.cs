using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using EnumRun.Lib;
using System.Threading;
using System.Diagnostics;
using EnumRun.Log;

namespace EnumRun
{
    internal class Script
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public int FileNumber { get; set; }
        public bool Enabled { get; set; }
        public EnumRunOption Option { get; set; }

        private EnumRunSetting _setting { get; set; }
        private Language _language { get; set; }
        private Logger _logger { get; set; }

        private static readonly Regex _pat_fileNum = new Regex(@"^\d+(?=_)");

        public Script() { }
        public Script(string filePath, EnumRunSetting setting, LanguageCollection collection, Logger logger)
        {
            this.FilePath = filePath;
            this.FileName = Path.GetFileName(filePath);

            Match match;
            this.FileNumber = (match = _pat_fileNum.Match(filePath)).Success ?
                int.Parse(match.Value) : -1;

            if (setting.Ranges.Within(this.FileNumber))
            {
                this.Enabled = true;
                this.Option = new EnumRunOption(this.FilePath);
                this._setting = setting;
                this._language = collection.GetLanguage(this.FilePath);
                this._logger = logger;

                _logger.Write(LogLevel.Info, FileName, "Enabled");
                _logger.Write(LogLevel.Debug, FileName, "Language:{0}", _language.ToString());
                _logger.Write(LogLevel.Debug, FileName, Option.ToString());
                //  [Log]Enable判定だったこと。
                //  [Log]Language判定
                //  [Log]含むオプション Option.ToString()
            }
        }

        public Task Process()
        {
            if (StopByOption()) { return null; }

            //  実行前待機
            if (this.Option.Contains(OptionType.BeforeWait) && Option.BeforeTime > 0)
            {
                //  [Log]実行前待機すること。秒
                Thread.Sleep(Option.BeforeTime * 1000);
            }

            //  終了待ち/標準出力有りの各パターンの組み合わせ
            //    終了待ち:false/標準出力:false ⇒ wait無し。別プロセスとして非管理で実行
            //    終了待ち:false/標準出力:true  ⇒ スレッド内でのみwait。全スレッド終了待ち
            //    終了待ち:true/標準出力:false  ⇒ スレッド内でもwait。スレッド呼び出し元でもwait
            //    終了待ち:true/標準出力:true   ⇒ スレオッド内でwait。スレッド呼び出し元でもwait
            Task task = this._setting.DefaultOutput || this.Option.Contains(OptionType.Output) ?
                ProcessThreadAndOutput() :
                ProcessThread();
            if (Option.Contains(OptionType.WaitForExit))
            {
                task.Wait();
            }

            //  実行後待機
            if (this.Option.Contains(OptionType.AfterWait) && Option.AfterTime > 0)
            {
                //  [Log]実行後待機すること。秒
                Thread.Sleep(Option.AfterTime * 1000);
            }

            return task;
        }

        /// <summary>
        /// オプションによって実行対象外と判定するかどうか
        /// </summary>
        /// <returns>実行対象外の場合にtrue</returns>
        private bool StopByOption()
        {
            //  [n]オプション
            //  実行対象外
            if (this.Option.Contains(OptionType.NoRun))
            {
                //  [Log]実行対象外ということ
                return true;
            }

            //  [m]オプション
            //  ドメイン参加PCのみ
            if (this.Option.Contains(OptionType.DomainPCOnly) && !Machine.IsDomain)
            {
                //  [Log]ドメイン参加していないということ
                //  [Log]ドメイン名。Machine.DomainName
                return true;
            }

            //  [k]オプション
            //  ワークグループPCのみ
            if (this.Option.Contains(OptionType.WorkgroupPCOnly) && Machine.IsDomain)
            {
                //  [log]ワークグループPCではないということ
                //  [Log]ワークグループ名。Machine.WorkgroupName
                return true;
            }

            //  [s]オプション
            //  システムアカウントのみ
            if (this.Option.Contains(OptionType.SystemAccountOnly) && !UserAccount.IsSystemAccount)
            {
                //  [Log]システムアカウントではないこと
                //  [Log]SIDを出力
                return true;
            }

            //  [d]オプション
            //  ドメインユーザーのみ
            if (this.Option.Contains(OptionType.DomainUserOnly) && !UserAccount.IsDomainUser)
            {
                //  [Log]ドメインユーザーではないこと(ローカルユーザーである)
                //  [Log]ユーザー名
                return true;
            }

            //  [l]オプション
            //  ローカルユーザーのみ
            if (this.Option.Contains(OptionType.LocalUserOnly) && UserAccount.IsDomainUser)
            {
                //  [Log]ローカルユーザーではないこと(ドメインユーザーである)
                //  [Log]ユーザー名
                return true;
            }

            //  [p]オプション
            //  デフォルトゲートウェイへの通信確認
            if (this.Option.Contains(OptionType.DGReachableOnly) && !Machine.IsReachableDefaultGateway())
            {
                //  [Log]デフォルトゲートウェイへ導通不可であること
                return true;
            }

            //  [t]オプション
            //  管理者実行しているかどうか。
            //  管理者として実行させるオプション[a]とは異なるので注意。
            if (this.Option.Contains(OptionType.TrustedOnly) && !UserAccount.IsRunAdministrator())
            {
                //  [Log]管理者実行していないこと
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
            await Task.Run(async () =>
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
        private async Task ProcessThreadAndOutput()
        {
            string outputPath = Path.Combine(
                this._setting.OutputPath,
                string.Format("{0}_{1}_{2}.txt",
                    this.FileName,
                    Environment.ProcessId,
                    DateTime.Now.ToString("yyyyMMddHHmmss")));
            ParentDirectory.Create(outputPath);

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
