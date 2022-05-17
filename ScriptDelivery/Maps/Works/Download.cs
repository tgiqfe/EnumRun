
namespace ScriptDelivery.Maps.Works
{
    internal class Download
    {
        [YamlDotNet.Serialization.YamlMember(Alias = "path")]
        public string Path { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "destination")]
        public string Destination { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "keep")]
        public string Keep { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "user")]
        public string UserName { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "password")]
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
