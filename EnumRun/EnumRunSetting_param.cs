using EnumRun.Lib;
using EnumRun.Log.ProcessLog;
using Hnx8.ReadJEnc;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using EnumRun.Log;
using EnumRun.Log.Syslog;

namespace EnumRun
{
    internal partial class EnumRunSetting
    {
        private string _FilesPath = null;
        private string _logsPath = null;
        private string _outputPath = null;

        #region Public parameter

        /// <summary>
        /// スクリプトファイルの保存先フォルダーのパス
        /// </summary>
        public string FilesPath
        {
            get { return _FilesPath ?? Path.Combine(Item.WorkDirectory, "Files"); }
            set { _FilesPath = value; }
        }

        /// <summary>
        /// ログ出力先フォルダーのパス
        /// </summary>
        public string LogsPath
        {
            get { return _logsPath ?? Path.Combine(Item.WorkDirectory, "Logs"); }
            set { this._logsPath = value; }
        }

        /// <summary>
        /// スクリプト実行時の標準出力の出力先パス
        /// </summary>
        public string OutputPath
        {
            get { return _outputPath ?? Path.Combine(Item.WorkDirectory, "Output"); }
            set { this._outputPath = value; }
        }

        /// <summary>
        /// 同じプロセスで次回実行可能になるまでの待ち時間(ループバックGPO対策) (秒)
        /// </summary>
        public int? RestTime { get; set; }

        /// <summary>
        /// デフォルトでスクリプト実行時の標準出力を出力させるかどうか
        /// </summary>
        public bool? DefaultOutput { get; set; }

        /// <summary>
        /// ログや標準出力の出力データの最大保持期間(日)
        /// </summary>
        public int? RetentionPeriod { get; set; }

        /// <summary>
        /// ログ出力の最低レベル
        /// </summary>
        public LogLevel? MinLogLevel { get; set; }

        /// <summary>
        /// ログ転送先サーバ(Logstash)のサーバ
        /// 記述例⇒http://192.168.10.100:8080/
        /// </summary>
        public string LogstashServer { get; set; }

        /// <summary>
        /// ログ転送先のSyslogサーバのアドレス
        /// 例)
        /// udp://192.168.1.100:514
        /// tcp://192.168.1.100:514
        /// </summary>
        public string SyslogServer { get; set; }

        /// <summary>
        /// アプリケーション内で送信するSyslogファシリティ
        /// </summary>
        public string SyslogFacility { get; set; }

        /// <summary>
        /// Syslog転送時のフォーマット
        /// RFC3164、RFC5424の2種類から選択可能。無指定の場合はRFC3164
        /// </summary>
        public string SyslogFormat { get; set; }

        /// <summary>
        /// TCP接続時、暗号化通信を有効にするかどうか
        /// </summary>
        public bool? SyslogSslEncrypt { get; set; }

        /// <summary>
        /// 暗号化通信時の接続施行タイムアウト時間(ミリ秒)
        /// </summary>
        public int? SyslogSslTimeout { get; set; }

        /// <summary>
        /// 暗号化通信時に使用する、クライアント証明書ファイルへのパス
        /// (.pfxファイル)
        /// </summary>
        public string SyslogSslCertFile { get; set; }

        /// <summary>
        /// クライアント証明書ファイルのパスワード(平文)
        /// </summary>
        public string SyslogSslCertPassword { get; set; }

        /// <summary>
        /// 証明書ストア内で使用する証明書のフレンドリ名
        /// パスワード記載したくない場合はこちらを使用することを推奨
        /// 証明書ストアは、[現在のユーザー][ローカルコンピュータ]の両方の、[個人]ストア配下を参照し、
        /// フレンドリ名が一致する証明書を使用
        /// </summary>
        public string SyslogSslCertFriendryName { get; set; }

        /// <summary>
        /// TCP接続で暗号化通信時、証明書チェックを無効化するかどうか
        /// 無効化していた場合、クライアント証明書は使用できないので注意
        /// </summary>
        public bool? SyslogSslIgnoreCheck { get; set; }

        /// <summary>
        /// プロセスごとに実行可能なスクリプトファイルの番号の範囲
        /// </summary>
        public ProcessRanges Ranges { get; set; }

        #endregion
    }
}
