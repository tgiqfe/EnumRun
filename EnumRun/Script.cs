using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using EnumRun.Lib;

namespace EnumRun
{
    internal class Script
    {
        public string FilePath { get; set; }
        public int FileNumber { get; set; }
        public Language Language { get; set; }
        public bool Enabled { get; set; }
        public EnumRunOption Option { get; set; }

        private static readonly Regex pattern_fileNum = new Regex(@"^\d+(?=_)");

        public Script() { }
        public Script(string filePath, EnumRunSetting setting, LanguageCollection collection)
        {
            this.FilePath = filePath;

            Match match;
            this.FileNumber = (match = pattern_fileNum.Match(filePath)).Success ?
                int.Parse(match.Value) : -1;

            if (setting.Ranges.Within(this.FileNumber))
            {
                this.Enabled = true;
                this.Language = collection.GetLanguage(this.FilePath);
                this.Option = new EnumRunOption(this.FilePath);

                //  [Log]Enable判定だったこと。
                //  [Log]Language判定
                //  [Log]含むオプション Option.ToString()
            }
        }

        public void Process()
        {
            //  [n]オプション
            //  実行対象外
            if (this.Option.Contains(OptionType.NoRun))
            {
                //  [Log]実行対象外ということ
                return;
            }

            //  [m]オプション
            //  ドメイン参加PCのみ
            if (this.Option.Contains(OptionType.DomainPCOnly) && !Machine.IsDomain)
            {
                //  [Log]ドメイン参加していないということ
                //  [Log]ドメイン名。Machine.DomainName
                return;
            }

            //  [k]オプション
            //  ワークグループPCのみ
            if (this.Option.Contains(OptionType.WorkgroupPCOnly) && Machine.IsDomain)
            {
                //  [log]ワークグループPCではないということ
                //  [Log]ワークグループ名。Machine.WorkgroupName
                return;
            }

            //  [s]オプション
            //  システムアカウントのみ
            if (this.Option.Contains(OptionType.SystemAccountOnly) && !UserAccount.IsSystemAccount)
            {
                //  [Log]システムアカウントではないこと
                //  [Log]SIDを出力
                return;
            }

            //  [d]オプション
            //  ドメインユーザーのみ
            if (this.Option.Contains(OptionType.DomainUserOnly) && !UserAccount.IsDomainUser)
            {
                //  [Log]ドメインユーザーではないこと(ローカルユーザーである)
                //  [Log]ユーザー名
                return;
            }

            //  [l]オプション
            //  ローカルユーザーのみ
            if (this.Option.Contains(OptionType.LocalUserOnly) && UserAccount.IsDomainUser)
            {
                //  [Log]ローカルユーザーではないこと(ドメインユーザーである)
                //  [Log]ユーザー名
                return;
            }

            //  [p]オプション
            //  デフォルトゲートウェイへの通信確認
            if(this.Option.Contains(OptionType.DGReachableOnly) && !Machine.IsReachableDefaultGateway())
            {
                //  [Log]デフォルトゲートウェイへ導通不可であること
                return;
            }

            //  [t]オプション
            //  管理者実行しているかどうか。
            //  管理者として実行させるオプション[a]とは異なるので注意。
            if (this.Option.Contains(OptionType.TrustedOnly) && !UserAccount.IsRunAdministrator())
            {
                //  [Log]管理者実行していないこと
                return;
            }








        }




    }
}
