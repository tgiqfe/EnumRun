namespace EnumRun
{
    internal class Item
    {
        /// <summary>
        /// 実行ファイルへのパス
        /// 単一実行ファイルにした場合、Assembly.Locationが使用できないので、Processからファイルパスを取得
        /// </summary>
        //public static readonly string AssemblyPath = Assembly.GetExecutingAssembly().Location;
        public static readonly string ExecFilePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

        /// <summary>
        /// 実行ファイルの名前(プロセス名)
        /// </summary>
        public static readonly string ProcessName = Path.GetFileNameWithoutExtension(ExecFilePath);

        /// <summary>
        /// 実行ファイルの場所
        /// </summary>
        public static readonly string ExecDirectoryPath = Path.GetDirectoryName(ExecFilePath);

        /// <summary>
        /// ワークフォルダー
        /// システムアカウントの場合は実行ファイルの場所。それ以外はTempフォルダー配下
        /// </summary>
        public static readonly string WorkDirectory =
            EnumRun.Lib.UserAccount.IsSystemAccount ?
                ExecDirectoryPath :
                Path.Combine(Path.GetTempPath(), "EnumRun");

        #region File

        /// <summary>
        /// セッション管理用ファイル
        /// </summary>
        public const string SESSION_FILE = "session.json";

        /// <summary>
        /// 旧ファイルのクリーンの管理用ファイル
        /// </summary>
        public const string CLEAN_FILE = "clean.json";

        /// <summary>
        /// 設定ファイル(JSON)
        /// </summary>
        public const string CONFIG_JSON = "Setting.json";

        /// <summary>
        /// 設定ファイル(TXT)
        /// </summary>
        public const string CONFIG_TXT = "Setting.txt";

        /// <summary>
        /// スクリプト言語設定ファイル
        /// </summary>
        public const string LANG_JSON = "Language.json";

        #endregion
    }
}
