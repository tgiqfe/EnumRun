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
        /// プロセスごとに実行可能なスクリプトファイルの番号の範囲
        /// </summary>
        public ParamRanges Ranges { get; set; }

        /// <summary>
        /// Lostash設定情報
        /// </summary>
        public ParamLogstash Logstash { get; set; }

        /// <summary>
        /// Syslog設定情報
        /// </summary>
        public ParamSyslog Syslog { get; set; }

        #endregion



    }
}
