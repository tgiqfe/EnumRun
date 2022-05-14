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
        private bool _reserve = false;
        private IStoredFileCollection _collection = null;

        public DirectoryWatcher(string targetPath, IStoredFileCollection collection)
        {
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
            if (_during || _reserve)
            {
                if (_reserve) { return; }
                _reserve = true;

                await Task.Delay(10000);
                _reserve = false;
            }

            _during = true;

            Item.Logger.Write(Logs.LogLevel.Info, null, "RecheckSource",
                "Recheck => {0}", _collection.GetType().Name);
            _collection.CheckSource();

            await Task.Delay(10000);        //  Recheckした後の待機時間 ⇒ 10秒
            _during = false;
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
