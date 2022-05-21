
namespace ScriptDelivery.Maps.Works
{
    internal class Work
    {
        [YamlDotNet.Serialization.YamlMember(Alias = "download")]
        public Download[] Downloads { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "delete"), Values("DeleteAction")]
        public DeleteFile Delete { get; set; }
    }
}
