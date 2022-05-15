using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptDelivery.Maps;
using ScriptDelivery.Maps.Requires;
using Microsoft.Win32;
using ScriptDelivery;
using EnumRun.Lib;
using EnumRun.Logs;

namespace EnumRun.ScriptDelivery.Maps.Matcher
{
    /// <summary>
    /// レジストリのマッチ確認
    /// - Equal    : 完全一致判定。ワイルドカード判定は無し
    /// </summary>
    internal class RegistryMatcher : MatcherBase
    {
        [MatcherParameter(Mandatory = true), Keys("Path", "RegistryKey")]
        public string Path { get; set; }

        [MatcherParameter(Mandatory = true), Keys("Name", "RegistryName")]
        public string Name { get; set; }

        [MatcherParameter(MandatoryAny = 1), Keys("Value", "RegistryValue")]
        public string Value { get; set; }

        [MatcherParameter(MandatoryAny = 2), Keys("RegistryType"), Values("RegistryType")]
        public RegistryValueKind? Type { get; set; }

        [MatcherParameter, Keys("NoExpand")]
        public bool? NoExpand { get; set; }

        public override bool IsMatch(RuleMatch ruleMatch)
        {
            bool ret = ruleMatch switch
            {
                RuleMatch.Equal => EqualMatch(),
                _ => false,
            };

            _logger.Write(ret ? LogLevel.Debug : LogLevel.Attention, $"MatchType => {ruleMatch}, Match => {ret}");

            return ret;
        }

        #region Match methods

        /// <summary>
        /// レジストリ値/レジストリタイプ一致判定
        /// </summary>
        /// <returns></returns>
        private bool EqualMatch()
        {
            using (var regKey = RegistryControl.GetRegistryKey(this.Path, false, false))
            {
                if (regKey == null) { return false; }

                RegistryValueKind valueKind = regKey.GetValueKind(this.Name);

                bool ret = true;
                if (this.Value != null)
                {
                    string text = RegistryControl.RegistryValueToString(
                        regKey,
                        Name,
                        valueKind,
                        (this.NoExpand ?? false) && valueKind == RegistryValueKind.ExpandString);
                    ret &= text.Equals(this.Value);
                }
                if (this.Type != null)
                {
                    ret &= Type == valueKind;
                }
                return ret;
            }
        }

        #endregion
    }
}
