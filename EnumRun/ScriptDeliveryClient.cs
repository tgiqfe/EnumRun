using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptDelivery.Maps.Requires;
using EnumRun.ScriptDelivery.Maps.Matcher;
using ScriptDelivery.Maps;
using System.Text.Json;
using System.Net;
using ScriptDelivery.Files;
using EnumRun.Logs;
using EnumRun.Lib.Infos;
using EnumRun.Lib;

namespace EnumRun
{
    internal class ScriptDeliveryClient
    {
        public bool Enabled { get; set; }

        private string _uri = null;
        private Logs.ProcessLog.ProcessLogger _logger = null;
        //private JsonSerializerOptions _options = null;
        private string _filesPath = null;

        private List<Mapping> _mappingList = null;
        //private List<string> _smbDownloadList = null;
        //private List<DownloadFile> _httpDownloadList = null;
        private ScriptDelivery.SmbDownloadManager _smbDownloadManager = null;
        private ScriptDelivery.HttpDownloadManager _httpDownloadManager = null;
        private ScriptDelivery.DeleteManager _deleteManager = null;

        //  後日、SmbとHttpのダウンロード用処理部分だけを別クラスに分離する予定。



        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ScriptDeliveryClient(EnumRunSetting setting, Logs.ProcessLog.ProcessLogger logger)
        {
            if (setting.ScriptDelivery != null &&
                (setting.ScriptDelivery.Process?.Equals(Item.ProcessName, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                //  指定サーバの候補からランダムに選択
                var random = new Random();
                string[] array = setting.ScriptDelivery.Server.OrderBy(x => random.Next()).ToArray();
                foreach (var sv in array)
                {
                    var info = new ServerInfo(sv, 5000, "http");
                    var connect = new TcpConnect(info.Server, info.Port);
                    if (connect.TcpConnectSuccess)
                    {
                        _uri = info.URI;
                        break;
                    }
                }

                this._logger = logger;
                /*
                this._options = new System.Text.Json.JsonSerializerOptions()
                {
                    //Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    IgnoreReadOnlyProperties = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    //WriteIndented = true,
                    //Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                };
                */
                this._filesPath = setting.GetFilesPath();

                _logger.Write(LogLevel.Info, null, "Connect server => {0}", _uri);

                if (!string.IsNullOrEmpty(_uri))
                {
                    this.Enabled = true;
                    //this._smbDownloadList = new List<string>();
                    this._smbDownloadManager = new ScriptDelivery.SmbDownloadManager();
                    //this._httpDownloadList = new List<DownloadFile>();
                    this._httpDownloadManager = new ScriptDelivery.HttpDownloadManager(_uri, _filesPath, _logger);
                    this._deleteManager = new ScriptDelivery.DeleteManager(setting.FilesPath, @"D:\Test\Trash");      //  trash先の設定は後日修正
                }
            }
        }

        public void StartDownload()
        {
            if (this.Enabled)
            {
                using (var client = new HttpClient())
                {
                    DownloadMappingFile(client).Wait();
                    MapMathcingCheck();

                    _smbDownloadManager.Process();
                    /*
                    
                    if (_smbDownloadList.Count > 0)
                    {
                        DownloadSmbFile();
                    }*/

                    _httpDownloadManager.Process(client);

                    /*
                    if (_httpDownloadList.Count > 0)
                    {
                        DownloadHttpSearch(client).Wait();
                        DownloadHttpStart(client).Wait();
                    }
                    */

                    _deleteManager.Process();

                    /*
                    _deleteManager.SearchTarget();
                    _deleteManager.DeleteTarget();
                    */
                }
            }
        }


        /// <summary>
        /// ScriptDeliveryサーバからMappingファイルをダウンロード
        /// </summary>
        /// <returns></returns>
        private async Task DownloadMappingFile(HttpClient client)
        {
            _logger.Write(LogLevel.Debug, "ScriptDelivery init.");
            using (var content = new StringContent(""))
            using (var response = await client.PostAsync(_uri + "/map", content))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    this._mappingList = JsonSerializer.Deserialize<List<Mapping>>(json);
                    _logger.Write(LogLevel.Info, "Success, download mapping object.");


                    var appVersion = response.Headers.FirstOrDefault(x => x.Key == "App-Version").Value.First();
                    var localVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    if (appVersion != localVersion)
                    {
                        _logger.Write(LogLevel.Warn, null, "AppVersion mismatch. server=>{0} local=>{1}", appVersion, localVersion);
                    }
                }
                else
                {
                    _logger.Write(LogLevel.Error, "Failed, download mapping object.");
                }
            }
        }

        /// <summary>
        /// MappingデータとローカルPC情報を確認し、ダウンロード対象のファイルを取得
        /// </summary>
        private void MapMathcingCheck()
        {
            _logger.Write(LogLevel.Debug, "Check, mapping object.");

            _mappingList = _mappingList.Where(x =>
            {
                RequireMode mode = x.Require.GetRequireMode();
                if (mode == RequireMode.None)
                {
                    return true;
                }
                IEnumerable<bool> results = x.Require.Rules.Select(y =>
                {
                    MatcherBase matcher = MatcherBase.Activate(y.GetRuleTarget());
                    matcher.SetLogger(_logger);
                    matcher.SetParam(y.Param);
                    return matcher.CheckParam() && (matcher.IsMatch(y.GetRuleMatch()) ^ y.GetInvert());
                });
                return mode switch
                {
                    RequireMode.And => results.All(x => x),
                    RequireMode.Or => results.Any(x => x),
                    _ => false,
                };
            }).ToList();

            _logger.Write(LogLevel.Debug, null, "Finish, require check [Match => {0} count]", _mappingList.Count);

            foreach (var mapping in _mappingList)
            {
                foreach (var download in mapping.Work.Downloads)
                {
                    if (string.IsNullOrEmpty(download.Path))
                    {
                        _logger.Write(LogLevel.Attention, null, "Parameter missing, Path parameter.");
                    }
                    else if (download.Path.StartsWith("\\\\"))
                    {
                        //  Smbダウンロード用ファイル
                        //_smbDownloadManager.～～～～
                        //  未実装
                    }
                    else
                    {
                        //  Htttpダウンロード用ファイル
                        _httpDownloadManager.DownloadList.Add(new DownloadFile()
                        {
                            Path = download.Path,
                            DestinationPath = download.Destination,
                            Overwrite = !download.GetKeep(),
                        });

                        /*
                        _httpDownloadList.Add(new DownloadFile()
                        {
                            Path = download.Path,
                            DestinationPath = download.Destination,
                            Overwrite = !download.GetKeep(),
                        });
                        */
                    }
                }
                if (mapping.Work.Delete != null)
                {
                    _deleteManager.Targetlist.AddRange(mapping.Work.Delete.DeleteTarget);
                    _deleteManager.ExcludeList.AddRange(mapping.Work.Delete.DeleteExclude);
                }
            }
        }

        /*
        /// <summary>
        /// Smbダウンロード
        /// </summary>
        private void DownloadSmbFile()
        {
            _logger.Write(LogLevel.Debug, "Search, download file from SMB server.");

            //  未実装
        }
        */

        /*
        /// <summary>
        /// Httpダウンロードする場合に、ScriptDeliveryサーバにダウンロード可能ファイルを問い合わせ
        /// </summary>
        /// <returns></returns>
        private async Task DownloadHttpSearch(HttpClient client)
        {
            _logger.Write(LogLevel.Debug, "Search, download file from ScriptDelivery server.");

            using (var content = new StringContent(
                 JsonSerializer.Serialize(_httpDownloadList, _options), Encoding.UTF8, "application/json"))
            using (var response = await client.PostAsync(_uri + "/download/list", content))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    _httpDownloadList = JsonSerializer.Deserialize<List<DownloadFile>>(json);

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

            foreach (var dlFile in _httpDownloadList)
            {
                string dstPath = string.IsNullOrEmpty(dlFile.DestinationPath) ?
                    Path.Combine(_filesPath, dlFile.Path) :
                    Path.Combine(dlFile.DestinationPath, dlFile.Path);

                //  ローカル側のファイルとの一致チェック
                if (!(dlFile.Downloadable ?? false)) { continue; }
                if (dlFile.CompareFile(dstPath) && !(dlFile.Overwrite ?? false))
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
        */
    }
}
