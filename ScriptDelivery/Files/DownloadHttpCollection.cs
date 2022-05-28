using System.IO;
using ScriptDelivery.Logs;
using ScriptDelivery.Lib;

namespace ScriptDelivery.Files
{
    public class DownloadHttpCollection : IStoredFileCollection
    {
        private List<DownloadHttp> _list = null;

        private string _baseDir = null;

        public DownloadHttpCollection() { }

        public DownloadHttpCollection(string filesPath)
        {
            _baseDir = filesPath;
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

            //  [案]HTTPダウンロード候補として格納しているリストを、外部から確認可能にする為に、ファイル出力も検討
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

