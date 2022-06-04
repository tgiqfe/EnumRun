using System.Text;
using System.IO;
using ScriptDelivery;
using System.Text.Json;
using System.Text.Json.Serialization;
using ScriptDelivery.Maps.Requires;
using ScriptDelivery.Maps.Works;
using ScriptDelivery.Files;

namespace ScriptDelivery.Files
{
    internal class MappingFileCollection : IStoredFileCollection
    {
        private List<MappingFile> _list = null;

        private string _baseDir = null;

        public string Content { get; set; }

        private string _storedFile = null;
        private JsonSerializerOptions _options = null;

        //public MappingFileCollection() { }

        public MappingFileCollection(string mapsPath, string logsPath)
        {
            _baseDir = mapsPath;
            _storedFile = Path.Combine(logsPath, "StoredMapping.json");
            _options = Item.GetJsonSerializerOption(
                escapeDoubleQuote: true,
                ignoreReadOnly: false,
                ignoreNull: false,
                writeIndented: true,
                convertEnumCamel: true);
            /*
            _options = new JsonSerializerOptions()
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                //IgnoreReadOnlyProperties = true,
                //DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };
            */
            CheckSource();
        }

        public void CheckSource()
        {
            string logTitle = "CheckSource";

            _list = new List<MappingFile>();
            if (Directory.Exists(_baseDir))
            {
                foreach (string file in Directory.GetFiles(_baseDir))
                {
                    _list.Add(new MappingFile(_baseDir, file));
                }
            }

            var mappingList = _list.SelectMany(x => x.MappingList.Select(y => y)).ToList();
            this.Content = JsonSerializer.Serialize(mappingList);

            Item.Logger.Write(Logs.LogLevel.Info, 
                null,
                logTitle,
                "MapFiles => [{0}]",
                string.Join(", ", _list.Select(x => x.Name)));

            //  格納済みデータを外部確認用に出力
            using (var sw = new StreamWriter(_storedFile, false, Encoding.UTF8))
            {
                string json = JsonSerializer.Serialize(_list, _options);
                sw.WriteLine(json);
            }
        }
    }
}
