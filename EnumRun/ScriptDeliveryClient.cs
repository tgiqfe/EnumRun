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
        private string _filesPath = null;

        private List<Mapping> _mappingList = null;
        private ScriptDelivery.SmbDownloadManager _smbDownloadManager = null;
        private ScriptDelivery.HttpDownloadManager _httpDownloadManager = null;
        private ScriptDelivery.DeleteManager _deleteManager = null;

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
                this._filesPath = setting.GetFilesPath();

                _logger.Write(LogLevel.Info, null, "Connect server => {0}", _uri);

                if (!string.IsNullOrEmpty(_uri))
                {
                    this.Enabled = true;
                    this._smbDownloadManager = new ScriptDelivery.SmbDownloadManager();
                    this._httpDownloadManager = new ScriptDelivery.HttpDownloadManager(
                        _uri, _filesPath, _logger);
                    this._deleteManager = new ScriptDelivery.DeleteManager(
                        setting.FilesPath, setting.ScriptDelivery.TrashPath);
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
                    _httpDownloadManager.Process(client);
                    _deleteManager.Process();
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
                        //  未実装
                    }
                    else
                    {
                        //  Htttpダウンロード用ファイル
                        _httpDownloadManager.Add(download.Path, download.Destination, !download.GetKeep());
                        /*
                        _httpDownloadManager.DownloadList.Add(new DownloadFile()
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
    }
}
