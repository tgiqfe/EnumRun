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
            //  変更後即再チェック。その後10秒待機した後にロック解除
            //  ロック中にもう一回変更が発生した場合、ロック解除後に再チェック。
            //  ロック解除待ちの間にさらにもう一回変更が発生した場合は即終了。
            //  最大3スレッドが同時に稼働する可能性有り。
            //
            //  [問題発生]
            //  変更イベントが発生した後、書き込みロックが完了していない間に再チェックスレッドを
            //  開始してしまった場合、IOException
            /*
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
            */

            //  [別アルゴリズムで実施]
            //  変更開始後にロック開始。10秒巻待機後に再チェック
            //  ロック中に変更があった場合は終了 (同時最大は2スレッドまで)
            //  IOException発生時、最初に戻る(ループさせる)
            if (_during) { return; }
            _during = true;
            while (_during)
            {
                try
                {
                    Item.Logger.Write(Logs.LogLevel.Info, null, "RecheckSource",
                        "Recheck => {0}", _collection.GetType().Name);
                    await Task.Delay(10000);
                    _collection.CheckSource();
                    _during = false;
                }
                catch (IOException e)
                {
                    Item.Logger.Write(Logs.LogLevel.Error, null, "RecheckResource", "IOException occurred.");
                    Item.Logger.Write(Logs.LogLevel.Error, null, "RecheckResource", e.Message);
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
