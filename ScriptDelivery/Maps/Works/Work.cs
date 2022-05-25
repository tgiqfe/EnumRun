
namespace ScriptDelivery.Maps.Works
{
    internal class Work
    {
        [System.Text.Json.Serialization.JsonPropertyName("download")]
        public Download[] Downloads { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("delete"), Values("DeleteAction")]
        public DeleteFile Delete { get; set; }
    }
}
