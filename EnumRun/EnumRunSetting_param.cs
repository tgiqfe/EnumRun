using EnumRun.Lib;
using EnumRun.Log.ProcessLog;
using Hnx8.ReadJEnc;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using EnumRun.Log;
using EnumRun.Lib.Syslog;

namespace EnumRun
{
    internal partial class EnumRunSetting
    {
        /// <summary>
        /// スクリプトファイルの保存先フォルダーのパス
        /// </summary>
        public string FilesPath { get; set; }

        /// <summary>
        /// ログ出力先フォルダーのパス
        /// </summary>
        public string LogsPath { get; set; }

        /// <summary>
        /// スクリプト実行時の標準出力の出力先パス
        /// </summary>
        public string OutputPath { get; set; }

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
        public string MinLogLevel { get; set; }

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
    }
}
