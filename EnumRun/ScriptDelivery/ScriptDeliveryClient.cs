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

namespace EnumRun.ScriptDelivery
{
    internal class ScriptDeliveryClient
    {
        private ScriptDeliverySession _session = null;
        private Logs.ProcessLog.ProcessLogger _logger = null;
        private string _filesPath = null;

        private List<Mapping> _mappingList = null;
        private SmbDownloader _smbDownloader = null;
        private HttpDownloader _httpDownloader = null;
        private DeleteManager _deleteManager = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ScriptDeliveryClient(ScriptDeliverySession session, string filesPath, string logsPath, string trashPath, Logs.ProcessLog.ProcessLogger logger)
        {
            this._session = session;

            if (session.EnableDelivery)
            {
                _logger = logger;
                
                _filesPath = filesPath;
                _smbDownloader = new SmbDownloader(_logger);
                _httpDownloader = new HttpDownloader(_filesPath, _logger);
                _deleteManager = new DeleteManager(filesPath, trashPath, _logger);
            }
        }

        public void StartDownload()
        {
            string logTitle = "StartDownload";
            _logger.Write(LogLevel.Info, logTitle, "Select ScriptDelivery server => {0}", _session.Uri);

            if (_session.EnableDelivery && _session.Enabled)
            {
                DownloadMappingFile(_session.Client).Wait();
                MapMathcingCheck();

                _smbDownloader.DownloadProcess();
                _httpDownloader.DownloadProcess(_session.Client, _session.Uri);
                _deleteManager.Process();
            }
        }

        /// <summary>
        /// ScriptDeliveryサーバからMappingファイルをダウンロード
        /// </summary>
        /// <returns></returns>
        private async Task DownloadMappingFile(HttpClient client)
        {
            string logTitle = "DownloadMappingFile";

            _logger.Write(LogLevel.Debug, logTitle, "ScriptDelivery init.");
            using (var content = new StringContent(""))
            //using (var response = await client.PostAsync(_uri + "/map", content))
            using (var response = await client.PostAsync(_session.Uri + "/map", content))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    _mappingList = JsonSerializer.Deserialize<List<Mapping>>(json);
                    _logger.Write(LogLevel.Info, logTitle, "Success, download mapping object.");

                    var appVersion = response.Headers.FirstOrDefault(x => x.Key == "App-Version").Value.First();
                    var localVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    if (appVersion != localVersion)
                    {
                        _logger.Write(LogLevel.Warn, logTitle, "AppVersion mismatch. server=>{0} local=>{1}", appVersion, localVersion);
                    }
                }
                else
                {
                    _logger.Write(LogLevel.Error, logTitle, "Failed, download mapping object.");
                }
            }
        }

        /// <summary>
        /// MappingデータとローカルPC情報を確認し、ダウンロード対象のファイルを取得
        /// </summary>
        private void MapMathcingCheck()
        {
            string logTitle = "MapMathcingCheck";

            _logger.Write(LogLevel.Debug, logTitle, "Check, mapping object.");

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
                    return matcher.CheckParam() && matcher.IsMatch(y.GetRuleMatch()) ^ y.GetInvert();
                });
                return mode switch
                {
                    RequireMode.And => results.All(x => x),
                    RequireMode.Or => results.Any(x => x),
                    _ => false,
                };
            }).ToList();

            _logger.Write(LogLevel.Debug, logTitle, "Finish, require check [Match => {0} count]", _mappingList.Count);

            foreach (var mapping in _mappingList)
            {
                foreach (var download in mapping.Work.Downloads)
                {
                    if (string.IsNullOrEmpty(download.Path))
                    {
                        _logger.Write(LogLevel.Attention, logTitle, "Parameter missing, Path parameter.");
                    }
                    else if (download.Path.StartsWith("\\\\"))
                    {
                        //  Smbダウンロード用ファイル
                        _smbDownloader.Add(download.Path, download.Destination, download.UserName, download.Password, !download.GetKeep());
                    }
                    else
                    {
                        //  Htttpダウンロード用ファイル
                        _httpDownloader.Add(download.Path, download.Destination, !download.GetKeep());
                    }
                }
                if (mapping.Work.Delete != null)
                {
                    _deleteManager.AddTarget(mapping.Work.Delete.DeleteTarget);
                    _deleteManager.AddExclude(mapping.Work.Delete.DeleteExclude);
                }
            }
        }
    }
}
