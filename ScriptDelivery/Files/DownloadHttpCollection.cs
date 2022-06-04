using System.Text;
using System.IO;
using ScriptDelivery.Logs;
using ScriptDelivery.Lib;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScriptDelivery.Files
{
    public class DownloadHttpCollection : IStoredFileCollection
    {
        private List<DownloadHttp> _list = null;

        private string _baseDir = null;

        private string _storedFile = null;
        private JsonSerializerOptions _options = null;

        //public DownloadHttpCollection() { }

        public DownloadHttpCollection(string filesPath, string logsPath)
        {
            _baseDir = filesPath;
            _storedFile = Path.Combine(logsPath, "StoredDownloadHttp.json");
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
            _options = Item.GetJsonSerializerOption(
                escapeDoubleQuote: true,
                ignoreReadOnly: false,
                ignoreNull: false,
                writeIndented: true,
                convertEnumCamel: true);
            CheckSource();
        }

        public void CheckSource()
        {
            string logTitle = "CheckSource";

            _list = new List<DownloadHttp>();
            if (Directory.Exists(_baseDir))
            {
                foreach (string file in Directory.GetFiles(_baseDir, "*", SearchOption.AllDirectories))
                {
                    _list.Add(new DownloadHttp(_baseDir, file));
                }
            }

            Item.Logger.Write(ScriptDelivery.Logs.LogLevel.Info,
                null,
                logTitle,
                "DownloadFiles => [{0}]",
                    string.Join(", ", _list.Select(x => x.Path)));

            //  格納済みデータを外部確認用に出力
            using (var sw = new StreamWriter(_storedFile, false, Encoding.UTF8))
            {
                string json = JsonSerializer.Serialize(_list, _options);
                sw.WriteLine(json);
            }
        }

        /// <summary>
        /// 受け取ったDownloadFileリストから、ダウンロード可否を確認
        /// </summary>
        /// <param name="reqList"></param>
        /// <returns></returns>
        public List<DownloadHttp> RequestToResponse(List<DownloadHttp> reqList)
        {
            var resList = new List<DownloadHttp>();
            foreach (DownloadHttp reqFile in reqList)
            {
                //  reqのDownloadFileインスタンスのPathが、_listのPathと一致している場合のチェック
                var findFile = _list.FirstOrDefault(x => x.Path == reqFile.Path);
                if (findFile != null)
                {
                    resList.Add(new DownloadHttp()
                    {
                        Path = findFile.Path,
                        LastWriteTime = findFile.LastWriteTime,
                        Hash = findFile.Hash,
                        Downloadable = true,
                        DestinationPath = reqFile.DestinationPath,
                        Overwrite = reqFile.Overwrite,
                    });
                    continue;
                }

                //  reqのDownloadFileインスタンスのPathが、_listのディレクトリと一致ししている場合のチェック
                var findFiles = _list.Where(x => x.Path.StartsWith(reqFile.Path + Path.DirectorySeparatorChar)).ToList();
                if (findFiles.Count > 0)
                {
                    findFiles.ToList().ForEach(x =>
                    {
                        resList.Add(new DownloadHttp()
                        {
                            Path = x.Path,
                            LastWriteTime = x.LastWriteTime,
                            Hash = x.Hash,
                            Downloadable = true,
                            DestinationPath = reqFile.DestinationPath,
                            Overwrite = reqFile.Overwrite,
                        });
                    });
                    continue;
                }

                //  reqのDownloadFileインスタンスのPathが、ワイルドカード指定の場合のチェック
                if (reqFile.Path.Contains("*"))
                {
                    var pattern = reqFile.Path.GetWildcardPattern();
                    _list.Where(x => pattern.IsMatch(x.Path)).
                        ToList().
                        ForEach(x =>
                        {
                            resList.Add(new DownloadHttp()
                            {
                                Path = x.Path,
                                LastWriteTime = x.LastWriteTime,
                                Hash = x.Hash,
                                Downloadable = true,
                                DestinationPath = reqFile.DestinationPath,
                                Overwrite = reqFile.Overwrite,
                            });
                        });
                    continue;
                }

                resList.Add(reqFile);
            }
            return resList;
        }
    }
}

