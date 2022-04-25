using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace EnumRun
{
    internal class Item
    {
        /// <summary>
        /// 実行ファイルへのパス
        /// </summary>
        public static readonly string AssemblyPath = Assembly.GetExecutingAssembly().Location;

        /// <summary>
        /// 実行ファイルの名前
        /// </summary>
        public static readonly string AssemblyFile = Path.GetFileName(AssemblyPath);

        /// <summary>
        /// 実行ファイルの場所
        /// </summary>
        public static readonly string AssemblyDirectory = Path.GetDirectoryName(AssemblyPath);

        /// <summary>
        /// ワークフォルダー
        /// システムアカウントの場合は実行ファイルの場所。それ以外はTempフォルダー配下
        /// </summary>
        public static readonly string WorkDirectory = 
            EnumRun.Lib.UserAccount.IsSystemAccount() ?
                AssemblyDirectory : 
                Path.Combine(Path.GetTempPath(), "EnumRun");

        #region File

        /// <summary>
        /// セッション管理用ファイル
        /// </summary>
        public const string SESSION_FILE = "session.json";

        /// <summary>
        /// 設定ファイル
        /// </summary>
        public const string CONFIG_JSON = "Setting.json";

        /// <summary>
        /// スクリプト言語設定ファイル
        /// </summary>
        public const string LANG_JSON = "Language.json";

        #endregion
    }
}
