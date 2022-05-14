using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumRun.Lib;
using ScriptDelivery.Maps;
using ScriptDelivery.Maps.Requires;
using EnumRun.Logs;

namespace EnumRun.ScriptDelivery.Maps.Matcher
{
    /// <summary>
    /// 環境変数のマッチ確認
    /// - Equal    : 完全一致
    /// </summary>
    internal class EnvMatcher : MatcherBase
    {
        [MatcherParameter(Mandatory = true), Keys("Name", "Key")]
        public string Name { get; set; }

        [MatcherParameter, Keys("Value")]
        public string Value { get; set; }

        [MatcherParameter, Keys("Location")]
        public EnvLocation? Location { get; set; }

        public enum EnvLocation
        {
            All = 0,
            Process = 1,
            User = 2,
            Machine = 3
        }

        public override bool IsMatch(RuleMatch ruleMatch)
        {
            this.Location ??= EnvLocation.All;

            bool ret =  ruleMatch switch
            {
                RuleMatch.Equal => EqualMatch(),
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
            string val = Location switch
            {
                EnvLocation.All => Environment.GetEnvironmentVariable(this.Name),
                EnvLocation.Process => Environment.GetEnvironmentVariable(this.Name, EnvironmentVariableTarget.Process),
                EnvLocation.User => Environment.GetEnvironmentVariable(this.Name, EnvironmentVariableTarget.User),
                EnvLocation.Machine => Environment.GetEnvironmentVariable(this.Name, EnvironmentVariableTarget.Machine),
                _ => null,
            };
            if (string.IsNullOrEmpty(this.Value))
            {
                //  Valueが空の場合、環境変数の有無チェックのみ
                return !string.IsNullOrEmpty(val);
            }
            else if (Value.Contains("*"))
            {
                return this.Value.GetWildcardPattern().IsMatch(val);
            }
            return val.Equals(this.Value);
        }

        #endregion
    }
}
