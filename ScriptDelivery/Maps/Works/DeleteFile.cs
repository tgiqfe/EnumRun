
namespace ScriptDelivery.Maps.Works
{
    public class DeleteFile
    {
        [YamlDotNet.Serialization.YamlMember(Alias = "target")]
        public string[] DeleteTarget { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "exclude")]
        public string[] DeleteExclude { get; set; }
    }
}
