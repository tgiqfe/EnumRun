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
                string.Join(", ", _list.Select(x => x.Name)));
        }

        /// <summary>
        /// 受け取ったDownloadFileリストから、ダウンロード可否を確認
        /// </summary>
        /// <param name="reqList"></param>
        /// <returns></returns>
        public List<DownloadFile> RequestToResponse(List<DownloadFile> reqList)
        {
            var resList = new List<DownloadFile>();
            foreach (DownloadFile dlFile in reqList)
            {
                var findDlFile = _list.FirstOrDefault(x => x.Name == dlFile.Name);
                if(findDlFile != null)
                {
                    findDlFile.Downloadable = true;
                    findDlFile.DestinationPath = dlFile.DestinationPath;
                    findDlFile.Overwrite = dlFile.Overwrite;
                    resList.Add(findDlFile);
                    continue;
                }
                _list.Where(x => x.Name.StartsWith(dlFile.Name + Path.DirectorySeparatorChar)).
                    ToList().
                    ForEach(x =>
                    {
                        x.Downloadable = true;
                        x.DestinationPath = dlFile.DestinationPath;
                        x.Overwrite = dlFile.Overwrite;
                        resList.Add(x);
                    });
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

