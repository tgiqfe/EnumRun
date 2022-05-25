using System.Threading.Tasks;
using System.IO;
using System;
using System.Linq;
using System.Threading;

namespace ScriptDelivery.Files
{
    /// <summary>
    /// 対象フォルダー配下のファイルの変更履歴を監視
    /// </summary>
    public class DirectoryWatcher : IDisposable
    {
        private FileSystemWatcher _watcher = null;
        private bool _during = false;
        private IStoredFileCollection _collection = null;

        public DirectoryWatcher(string targetPath, IStoredFileCollection collection)
        {
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            _watcher = new FileSystemWatcher();
            _watcher.Path = targetPath;
            _watcher.NotifyFilter = NotifyFilters.LastWrite |
                NotifyFilters.FileName |
                NotifyFilters.DirectoryName;
            _watcher.IncludeSubdirectories = true;

            _watcher.Created += new FileSystemEventHandler((sender, e) => RecheckResource());
            _watcher.Changed += new FileSystemEventHandler((sender, e) => RecheckResource());
            _watcher.Deleted += new FileSystemEventHandler((sender, e) => RecheckResource());
            _watcher.Renamed += new RenamedEventHandler((sender, e) => RecheckResource());

            _watcher.EnableRaisingEvents = true;
            _collection = collection;
        }

        private async void RecheckResource()
        {
            string logTitle = "RecheckResource";

            //  変更開始後にロック開始。10秒巻待機後に再チェック
            //  ロック中に変更があった場合は終了 (同時最大は2スレッドまで)
            //  IOException発生時、最初に戻る(ループさせる)
            if (_during) { return; }
            _during = true;
            while (_during)
            {
                try
                {
                    Item.Logger.Write(Logs.LogLevel.Info,
                        null,
                        logTitle,
                        "Recheck => {0}",
                            _collection.GetType().Name);
                    await Task.Delay(10000);
                    _collection.CheckSource();
                    _during = false;
                }
                catch (IOException e)
                {
                    Item.Logger.Write(Logs.LogLevel.Error, logTitle, "IOException occurred.");
                    Item.Logger.Write(Logs.LogLevel.Error, logTitle, e.Message);
                }
            }
        }

        public void Close()
        {
            if (_watcher != null) { _watcher.Dispose(); }
        }

        #region Disposable

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Close();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
