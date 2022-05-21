
namespace ScriptDelivery.Maps.Requires
{
    internal class Require
    {
        [YamlDotNet.Serialization.YamlMember(Alias = "mode")]
        [Values("Mode")]
        public string Mode { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "rule")]
        public RequireRule[] Rules { get; set; }

        public RequireMode GetRequireMode()
        {
            return ValuesAttribute.GetEnumValue<RequireMode>(
                this.GetType().GetProperty("Mode"), this.Mode);
        }
    }
}
