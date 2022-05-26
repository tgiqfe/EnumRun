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

        public MappingFileCollection() { }

        public MappingFileCollection(string mapsPath)
        {
            _baseDir = mapsPath;
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
        }
    }
}
