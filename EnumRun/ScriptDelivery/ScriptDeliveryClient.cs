﻿using System;
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
        //public bool Enabled { get; set; }

        //private string _uri = null;

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
        /*
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

                _logger = logger;
                _filesPath = setting.GetFilesPath();

                _logger.Write(LogLevel.Info, null, "Connect server => {0}", _uri);

                if (!string.IsNullOrEmpty(_uri))
                {
                    Enabled = true;
                    _smbDownloader = new SmbDownloader(_logger);
                    //this._httpDownloader = new ScriptDelivery.HttpDownloader(
                    //    _uri, _filesPath, _logger);
                    _httpDownloader = new HttpDownloader(_filesPath, _logger);
                    _deleteManager = new DeleteManager(
                        setting.FilesPath, setting.ScriptDelivery.TrashPath, _logger);
                }
            }
        }
        */

        public ScriptDeliveryClient(ScriptDeliverySession session, string filesPath, string logsPath, string trashPath, Logs.ProcessLog.ProcessLogger logger)
        {
            this._session = session;

            if (session.EnableDelivery)
            {
                _logger = logger;
                _logger.Write(LogLevel.Info, null, "Connect server => {0}", session.Uri);

                _filesPath = filesPath;
                _smbDownloader = new SmbDownloader(_logger);
                _httpDownloader = new HttpDownloader(_filesPath, _logger);
                _deleteManager = new DeleteManager(filesPath, trashPath, _logger);
            }
        }

        public void StartDownload()
        {
            //if (Enabled)
            if (_session.EnableDelivery && _session.Enabled)
            {
                DownloadMappingFile(_session.Client).Wait();
                MapMathcingCheck();

                _smbDownloader.Process();
                //_httpDownloader.Process(client);
                //_httpDownloader.Process(client, _uri);
                _httpDownloader.Process(_session.Client, _session.Uri);
                _deleteManager.Process();
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
            //using (var response = await client.PostAsync(_uri + "/map", content))
            using (var response = await client.PostAsync(_session.Uri + "/map", content))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    _mappingList = JsonSerializer.Deserialize<List<Mapping>>(json);
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
                    return matcher.CheckParam() && matcher.IsMatch(y.GetRuleMatch()) ^ y.GetInvert();
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
