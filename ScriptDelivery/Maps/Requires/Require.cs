
namespace ScriptDelivery.Maps.Requires
{
    internal class Require
    {
        [System.Text.Json.Serialization.JsonPropertyName("mode")]
        [Values("Mode")]
        public string Mode { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("rule")]
        public RequireRule[] Rules { get; set; }

        public RequireMode GetRequireMode()
        {
            return ValuesAttribute.GetEnumValue<RequireMode>(
                this.GetType().GetProperty("Mode"), this.Mode);
        }
    }
}
