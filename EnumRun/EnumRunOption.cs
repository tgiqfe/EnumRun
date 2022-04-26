using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace EnumRun
{
    internal class EnumRunOption
    {
        /// <summary>
        /// スクリプト実行時オプション
        /// </summary>
        public OptionType OptionType { get; set; }

        /// <summary>
        /// スクリプト実行前の待ち時間(秒)
        /// </summary>
        public int BeforeTime { get; set; }

        /// <summary>
        /// スクリプト実行後の待ち時間(秒)
        /// </summary>
        public int AfterTime { get; set; }

        private static readonly Regex pattern_option = new Regex(@"(\[[0-9a-zA-Z]+\])+(?=\.[^.]+$)");
        private static readonly Regex pattern_befWait = new Regex(@"\d{1,3}(?=r)");
        private static readonly Regex pattern_aftWait = new Regex(@"(?<=r)\d{1,3}");

        /// <summary>
        /// コンストラクタ (引数無し)
        /// </summary>
        public EnumRunOption() { }

        /// <summary>
        /// コンストラクタ (ファイル名を指定)
        /// </summary>
        /// <param name="filePath"></param>
        public EnumRunOption(string filePath)
        {
            string fileName = Path.GetFileName(filePath).ToLower();

            OptionType = OptionType.None;

            Match match;
            if ((match = pattern_option.Match(fileName)).Success)
            {
                string matchText = match.Value;

                if (matchText.Contains("n")) { OptionType |= OptionType.NoRun; }
                if (matchText.Contains("w")) { OptionType |= OptionType.WaitForExit; }
                if (matchText.Contains("a")) { OptionType |= OptionType.RunAsAdmin; }
                if (matchText.Contains("m")) { OptionType |= OptionType.DomainPCOnly; }
                if (matchText.Contains("k")) { OptionType |= OptionType.WorkgroupPCOnly; }
                if (matchText.Contains("s")) { OptionType |= OptionType.SystemAccountOnly; }
                if (matchText.Contains("d")) { OptionType |= OptionType.DomainUserOnly; }
                if (matchText.Contains("l")) { OptionType |= OptionType.LocalUserOnly; }
                if (matchText.Contains("p")) { OptionType |= OptionType.DGReachableOnly; }
                if (matchText.Contains("t")) { OptionType |= OptionType.TrustedOnly; }
                if (matchText.Contains("o")) { OptionType |= OptionType.Output; }

                if ((match = pattern_befWait.Match(matchText)).Success)
                {
                    BeforeTime = int.Parse(match.Value);
                    OptionType |= OptionType.BeforeWait;
                }
                if ((match = pattern_aftWait.Match(matchText)).Success)
                {
                    AfterTime = int.Parse(match.Value);
                    OptionType |= OptionType.AfterWait;
                    OptionType |= OptionType.WaitForExit;
                }
            }
        }

        /// <summary>
        /// 対象のオプションが含まれているかどうかの判定
        /// </summary>
        /// <param name="targetOption"></param>
        /// <returns></returns>
        public bool Contains(OptionType targetOption)
        {
            //  ↓前の判定方法。でもこちらのほうがパフォーマンスは良いらしい
            //return (this.OptionType & targetOption) == targetOption;

            return this.OptionType.HasFlag(targetOption);
        }
    }
}
