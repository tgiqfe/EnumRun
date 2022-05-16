
namespace ScriptDelivery.Maps.Works
{
    public class DeleteFile
    {
        [YamlDotNet.Serialization.YamlMember(Alias = "action")]
        [Values("DeleteAction")]
        public string DeleteAction { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "target")]
        public string[] DeleteTarget { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "exclude")]
        public string[] DeleteExclude { get; set; }

        public DeleteAction GetDeleteAction()
        {
            return ValuesAttribute.GetEnumValue<DeleteAction>(
                this.GetType().GetProperty("DeleteAction"), this.DeleteAction);
        }
    }
}
