using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumRun.Lib;
using ScriptDelivery.Maps;
using ScriptDelivery.Maps.Requires;
using EnumRun.Logs;
using EnumRun.Lib.Infos;

namespace EnumRun.ScriptDelivery.Maps.Matcher
{
    /// <summary>
    /// ホスト名のマッチ確認
    /// - Equal     : 完全一致判定。ワイルドカードを含む場合は、Contains,StartsWith,EndsWithに対応
    /// - Range     : ホスト名末尾の数字部分のみの範囲を判定。数字部分のみを使用して判定する為、数字以外が異なっていても影響無し
    /// - NameRange : ホスト名末尾の数字部分を使用しての範囲判定。数字部分以外も含めて判定。
    /// 末尾が数字以外の場合、数字より後は無視
    /// </summary>
    internal class HostNameMatcher : MatcherBase
    {
        [MatcherParameter(Mandatory = true), Keys("Name")]
        public string Name { get; set; }

        [MatcherParameter, Keys("Start")]
        public string StartName { get; set; }

        [MatcherParameter, Keys("End")]
        public string EndName { get; set; }

        public override bool IsMatch(RuleMatch ruleMatch)
        {
            bool ret = ruleMatch switch
            {
                RuleMatch.Equal => EqualMatch(),
                RuleMatch.Range => RangeMatch(),
                RuleMatch.NameRange => NameRangeMatch(),
                _ => false,
            };

            _logger.Write(ret ? LogLevel.Debug : LogLevel.Attention, $"MatchType => {ruleMatch}, Match => {ret}");

            return ret;
        }

        #region Match methods

        /// <summary>
        /// Equal確認
        /// </summary>
        /// <returns></returns>
        private bool EqualMatch()
        {
            if (Name.Contains("*"))
            {
                return this.Name.GetWildcardPattern().IsMatch(Environment.MachineName);
            }
            return Environment.MachineName.Equals(Name, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 数字部分の範囲確認
        /// </summary>
        /// <returns></returns>
        private bool RangeMatch()
        {
            var info = new HostNameInfo(Environment.MachineName);
            int startNum = int.TryParse(this.StartName, out int tempStart) ? tempStart : 0;
            int endNum = int.TryParse(this.EndName, out int tempEnd) ? tempEnd : 0;

            return info.Number >= startNum && info.Number <= endNum;
        }

        /// <summary>
        /// 名前部分の一致と、数字部分の範囲確認
        /// </summary>
        /// <returns></returns>
        private bool NameRangeMatch()
        {
            var currentInfo = new HostNameInfo(Environment.MachineName);
            var startInfo = new HostNameInfo(this.StartName);
            var endInfo = new HostNameInfo(this.EndName);

            return currentInfo.PreName.Equals(startInfo.PreName, StringComparison.OrdinalIgnoreCase) &&
                currentInfo.PreName.Equals(endInfo.PreName, StringComparison.OrdinalIgnoreCase) &&
                currentInfo.Number >= startInfo.Number &&
                currentInfo.Number <= endInfo.Number;
        }

        #endregion
    }
}
