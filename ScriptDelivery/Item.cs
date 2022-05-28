using System;
using System.Text;
using ScriptDelivery;
using System.Runtime.InteropServices;
using System.Diagnostics;
using ScriptDelivery.Logs.DynamicLog;
using ScriptDelivery.Logs.ServerLog;

/// <summary>
/// 静的パラメータを格納
/// </summary>
namespace ScriptDelivery
{
    internal class Item
    {
        #region Path

        /// <summary>
        /// 実行ファイルへのパス
        /// </summary>
        public static readonly string ExecFilePath = Process.GetCurrentProcess().MainModule.FileName;

        /// <summary>
        /// 実行ファイルの名前(プロセス名)
        /// </summary>
        public static readonly string ProcessName = "ScriptDelivery";

        /// <summary>
        /// 実行ファイルの場所
        /// </summary>
        public static readonly string ExecDirectoryPath = Path.GetDirectoryName(ExecFilePath);

        #endregion
        #region Serial
        private static string _serial = null;

        public static string Serial
        {
            get
            {
                if (_serial == null)
                {
                    var md5 = System.Security.Cryptography.MD5.Create();
                    var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(
                        DateTime.Now.ToString() + Environment.MachineName + Process.GetCurrentProcess().Id.ToString()));
                    _serial = BitConverter.ToString(bytes).Replace("-", "");
                    md5.Clear();
                }
                return _serial;
            }
        }
        #endregion

        /// <summary>
        /// 実行中のOSの判定
        /// </summary>
        public static Platform Platform { get; set; }

        /// <summary>
        /// アプリケーション全体の制御情報
        /// </summary>
        public static Setting Setting { get; set; }

        /// <summary>
        /// アプリケーションの現在のバージョン
        /// </summary>
        public static string CurrentVersion { get; set; }

        /// <summary>
        /// ログ出力用
        /// </summary>
        public static ServerLogger Logger { get; set; }

        /// <summary>
        /// DynamicLog受信/出力用
        /// </summary>
        public static DynamicLogReceiver Receiver { get; set; }

        /// <summary>
        /// Mappingのリストとそれらの操作
        /// </summary>
        public static Files.MappingFileCollection MappingFileCollection { get; set; }

        /// <summary>
        /// ダウンロード対象ファイルのリスト
        /// </summary>
        public static Files.DownloadHttpCollection DownloadFileCollection { get; set; }
    }
}
