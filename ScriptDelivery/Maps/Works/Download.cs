
namespace ScriptDelivery.Maps.Works
{
    internal class Download
    {
        [System.Text.Json.Serialization.JsonPropertyName("path")]
        public string Path { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("destination")]
        public string Destination { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("keep")]
        public string Keep { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("user")]
        public string UserName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("password")]
        public string Password { get; set; }

        public bool GetKeep()
        {
            return this.Keep == null ?
                false :
                new string[]
                {
                    "", "0", "-", "false", "fals", "no", "not", "none", "non", "empty", "null", "否", "不", "無", "dis", "disable", "disabled"
                }.All(x => !x.Equals(this.Keep, StringComparison.OrdinalIgnoreCase));
        }
    }
}
