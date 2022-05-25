using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Reflection;

namespace ScriptDelivery.Files
{
    /// <summary>
    /// ダウンロードファイルについての情報を格納
    /// サーバ側/クライアント側の両方で使用。
    /// </summary>
    public class DownloadHttp
    {
        [JsonIgnore]
        public string FullPath { get; set; }        //  サーバ側でのみ使用。対象ファイルへの絶対パス
        public string Path { get; set; }            //  サーバ側の、Setting.FilesPathからの相対パス。大文字/小文字は区別
        public DateTime? LastWriteTime { get; set; } //  サーバ側のファイルの更新日時
        public string Hash { get; set; }            //  サーバ側のファイルのMD5ハッシュ値
        public bool? Downloadable { get; set; }     //  サーバ側に対象のファイルが存在し、ダウンロードが可能かどうか

        public string DestinationPath { get; set; } //  クライアント側のファイルのダウンロード先ファイル名。フォルダー名指定は非対応
        public bool? Overwrite { get; set; }        //  クライアント側で上書き保存を許可するかどうか

        public DownloadHttp() { }
        public DownloadHttp(string basePath, string filePath)
        {
            this.FullPath = System.IO.Path.GetFullPath(filePath);
            this.Path = System.IO.Path.GetRelativePath(basePath, filePath);
            this.LastWriteTime = File.GetLastWriteTime(filePath);
            this.Hash = GetHash(filePath);
        }

        /// <summary>
        /// ファイルのハッシュ化
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string GetHash(string filePath)
        {
            string ret = null;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var md5 = MD5.Create();
                byte[] bytes = md5.ComputeHash(fs);
                ret = BitConverter.ToString(bytes);
                md5.Clear();
            }
            return ret;
        }

        /// <summary>
        /// 対象ファイルと比較
        /// </summary>
        /// <param name="baseDir">true⇒一致。false⇒不一致</param>
        /// <returns></returns>
        public bool CompareFile(string path)
        {
            return (File.GetLastWriteTime(path) == this.LastWriteTime) &&
                (GetHash(path) == this.Hash);
        }

        public string ToLog()
        {
            var props = this.GetType().GetProperties(
                BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance).
                Where(x => x.Name != "FullPath");
            return string.Join(", ",
                props.Select(x => x.Name + " => " + x.GetValue(this)?.ToString()));
        }
    }
}
