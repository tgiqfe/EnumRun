
namespace ScriptDelivery.Maps.Requires
{
    internal class RequireRule
    {
        [YamlDotNet.Serialization.YamlMember(Alias = "target")]
        [Values("Target")]
        public string Target { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "match")]
        [Values("Match")]
        public string Match { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "invert")]
        public string Invert { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "param")]
        public Dictionary<string, string> Param { get; set; }

        public RuleTarget GetRuleTarget()
        {
            return ValuesAttribute.GetEnumValue<RuleTarget>(
                this.GetType().GetProperty("Target"), this.Target);
        }

        public RuleMatch GetRuleMatch()
        {
            return ValuesAttribute.GetEnumValue<RuleMatch>(
                this.GetType().GetProperty("Match"), this.Match);
        }

        public bool GetInvert()
        {
            return this.Invert == null ?
                false :
                new string[]
                {
                    "", "0", "-", "false", "fals", "no", "not", "none", "non", "empty", "null", "否", "不", "無", "dis", "disable", "disabled"
                }.All(x => !x.Equals(this.Invert, StringComparison.OrdinalIgnoreCase));
        }
    }
}
