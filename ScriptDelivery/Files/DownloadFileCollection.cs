using System.IO;
using ScriptDelivery.Logs;

namespace ScriptDelivery.Files
{
    public class DownloadFileCollection : IStoredFileCollection
    {
        private List<DownloadFile> _list = null;

        private string _baseDir = null;

        public DownloadFileCollection() { }

        public DownloadFileCollection(string filesPath)
        {
            _baseDir = filesPath;
            CheckSource();
        }

        public void CheckSource()
        {
            _list = new List<DownloadFile>();
            if (Directory.Exists(_baseDir))
            {
                foreach (string file in Directory.GetFiles(_baseDir, "*", SearchOption.AllDirectories))
                {
                    _list.Add(new DownloadFile(_baseDir, file));
                }
            }

            Item.Logger.Write(ScriptDelivery.Logs.LogLevel.Info, null, "DownloadFileList", "DownloadFiles => [{0}]",
                string.Join(", ", _list.Select(x => x.Path)));
        }

        /// <summary>
        /// 受け取ったDownloadFileリストから、ダウンロード可否を確認
        /// </summary>
        /// <param name="reqList"></param>
        /// <returns></returns>
        public List<DownloadFile> RequestToResponse(List<DownloadFile> reqList)
        {
            var resList = new List<DownloadFile>();
            foreach (DownloadFile reqFile in reqList)
            {
                //  Collectionインスタンスに格納しているリストを直接編集している為、
                //  複数の指定内容の異なるアクセスがあった場合に、設定が正しく反映されない可能性大
                /*
                var findDlFile = _list.FirstOrDefault(x => x.Path == dlFile.Path);
                if(findDlFile != null)
                {
                    findDlFile.Downloadable = true;
                    //findDlFile.DestinationPath = dlFile.DestinationPath;
                    findDlFile.Overwrite = dlFile.Overwrite;
                    resList.Add(findDlFile);
                    continue;
                }
                _list.Where(x => x.Path.StartsWith(dlFile.Path + Path.DirectorySeparatorChar)).
                    ToList().
                    ForEach(x =>
                    {
                        x.Downloadable = true;
                        //x.DestinationPath = dlFile.DestinationPath;
                        x.Overwrite = dlFile.Overwrite;
                        resList.Add(x);
                    });
                */

                //  修正。
                //  毎回全Downloadインスタンスを生成する方針で
                var findFile = _list.FirstOrDefault(x => x.Path == reqFile.Path);
                if (findFile != null)
                {
                    resList.Add(new DownloadFile()
                    {
                        Path = findFile.Path,
                        LastWriteTime = findFile.LastWriteTime,
                        Hash = findFile.Hash,
                        Downloadable = true,
                        Overwrite = reqFile.Overwrite,
                    });
                    continue;
                }
                var findFiles = _list.Where(x => x.Path.StartsWith(reqFile.Path + Path.DirectorySeparatorChar)).ToList();
                if (findFiles.Count > 0)
                {
                    findFiles.ToList().ForEach(x =>
                    {
                        resList.Add(new DownloadFile()
                        {
                            Path = x.Path,
                            LastWriteTime = x.LastWriteTime,
                            Hash = x.Hash,
                            Downloadable = true,
                            Overwrite = reqFile.Overwrite,
                        });
                    });
                    continue;
                }
                resList.Add(reqFile);
            }
            return resList;

            /*
            reqList.ForEach(x =>
            {
                var dlFile = _list.FirstOrDefault(y => y.Name == x.Name);
                if (dlFile != null)
                {
                    x.Downloadable = true;
                    x.LastWriteTime = dlFile.LastWriteTime;
                    x.Hash = dlFile.Hash;
                }
            });
            */
        }
    }
}

