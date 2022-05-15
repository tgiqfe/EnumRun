using System;
using System.Text;
using ScriptDelivery;
using System.Runtime.InteropServices;
using System.Diagnostics;
using ScriptDelivery.Files;
using ScriptDelivery.Logs;

namespace ScriptDelivery
{
    public class SessionWorker : IDisposable
    {
        private DirectoryWatcher _mappingFileWatcher = null;
        private DirectoryWatcher _downloadFileWatcher = null;

        public SessionWorker()
        {
            OnStart();
        }

        /// <summary>
        /// アプリケーション実行開始時の動作
        /// </summary>
        private void OnStart()
        {
            //  OSチェック
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Item.Platform = Platform.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Item.Platform = Platform.Linux;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Item.Platform = Platform.MacOSX;
            }

            //  設定ファイル読み込み
            Item.Setting = Setting.Deserialize("setting.json");
            Item.Setting.ChangePath();

            //  ログ出力開始
            Item.Logger = new Logs.ServerLogger(Item.Setting);

            //  Mappingリストを取得
            var mappingFileCollection = new MappingFileCollection(Item.Setting.MapsPath);
            _mappingFileWatcher = new DirectoryWatcher(Item.Setting.MapsPath, mappingFileCollection);
            Item.MappingFileCollection = mappingFileCollection;

            //  ダウンロードリストを取得
            var downloadFileCollection = new DownloadFileCollection(Item.Setting.FilesPath);
            _downloadFileWatcher = new DirectoryWatcher(Item.Setting.FilesPath, downloadFileCollection);
            Item.DownloadFileCollection = downloadFileCollection;

            //  アプリケーションバージョン
            Item.CurrentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        /// アプリケーション終了時の動作
        /// </summary>
        private void OnStop()
        {
            _mappingFileWatcher.Dispose();
            _downloadFileWatcher.Dispose();

            //  ログ出力終了
            if (Item.Logger != null)
            {
                Item.Logger.Dispose();
                Item.Logger = null;
            }
        }

        #region Dispose

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    OnStop();
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
