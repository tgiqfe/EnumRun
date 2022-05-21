using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptDelivery.Files;
using EnumRun.Logs;
using EnumRun.Logs.ProcessLog;
using System.Text.Json;
using System.Net;
using EnumRun.Lib;

namespace EnumRun.ScriptDelivery
{
    internal class HttpDownloader
    {
        private string _uri = null;
        private string _filesPath = null;
        private ProcessLogger _logger = null;
        private JsonSerializerOptions _options = null;
        private List<DownloadFile> _list = null;

        public HttpDownloader(string uri, string filesPath, ProcessLogger logger)
        {
            this._uri = uri;
            this._filesPath = filesPath;
            this._logger = logger;
            this._options = new System.Text.Json.JsonSerializerOptions()
            {
                //Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                IgnoreReadOnlyProperties = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                //WriteIndented = true,
                //Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };
            this._list = new List<DownloadFile>();
        }

        public void Add(string path, string destination, bool? overwrite)
        {
            _list.Add(new DownloadFile()
            {
                Path = path,
                DestinationPath = destination,
                Overwrite = overwrite,
            });
        }

        public void Process(HttpClient client)
        {
            if (this._list.Count > 0)
            {
                DownloadHttpSearch(client).Wait();
                DownloadHttpStart(client).Wait();
            }
        }

        /// <summary>
        /// Httpダウンロードする場合に、ScriptDeliveryサーバにダウンロード可能ファイルを問い合わせ
        /// </summary>
        /// <returns></returns>
        private async Task DownloadHttpSearch(HttpClient client)
        {
            _logger.Write(LogLevel.Debug, "Search, download file from ScriptDelivery server.");

            using (var content = new StringContent(
                 JsonSerializer.Serialize(_list, _options), Encoding.UTF8, "application/json"))
            using (var response = await client.PostAsync(_uri + "/download/list", content))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    _list = JsonSerializer.Deserialize<List<DownloadFile>>(json);

                    _logger.Write(LogLevel.Info, "Success, download DownloadFile list object");
                }
                else
                {
                    _logger.Write(LogLevel.Error, "Failed, download DownloadFile list object");
                }
            }
        }

        /// <summary>
        /// ScriptDeliveryサーバからファイルダウンロード
        /// </summary>
        /// <returns></returns>
        private async Task DownloadHttpStart(HttpClient client)
        {
            _logger.Write(LogLevel.Debug, "Start, Http download.");

            foreach (var dlFile in _list)
            {
                string dstPath = string.IsNullOrEmpty(dlFile.DestinationPath) ?
                    Path.Combine(_filesPath, Path.GetFileName(dlFile.Path)) :
                    Path.Combine(dlFile.DestinationPath, Path.GetFileName(dlFile.Path));

                //  ローカル側のファイルとの一致チェック
                if (!(dlFile.Downloadable ?? false)) { continue; }
                if (File.Exists(dstPath) &&
                    (dlFile.CompareFile(dstPath) || !(dlFile.Overwrite ?? false)))
                {
                    _logger.Write(LogLevel.Info, null, "Skip download, already exist. => [{0}]", dstPath);
                    continue;
                }
                TargetDirectory.CreateParent(dstPath);

                //  ダウンロード要求を送信し、ダウンロード開始
                var query = new Dictionary<string, string>()
                {
                    { "fileName", dlFile.Path }
                };
                using (var response = await client.GetAsync(_uri + $"/download/files?{await new FormUrlEncodedContent(query).ReadAsStringAsync()}"))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fs = new FileStream(dstPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            stream.CopyTo(fs);
                        }
                        File.SetLastWriteTime(dstPath, dlFile.LastWriteTime);
                        _logger.Write(LogLevel.Info, null, "Success, file download. [{0}]", dstPath);
                    }
                    else
                    {
                        _logger.Write(LogLevel.Info, null, "Failed, file download. [{0}]", dstPath);
                    }
                }
            }
        }
    }
}
