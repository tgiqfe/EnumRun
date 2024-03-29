﻿using System;
using System.Text;
using System.Diagnostics;
using EnumRun.Lib.Infos;

namespace EnumRun
{
    internal class Item
    {
        #region Path

        /// <summary>
        /// 実行ファイルへのパス
        /// 単一実行ファイルにした場合、Assembly.Locationが使用できないので、Processからファイルパスを取得
        /// </summary>
        //public static readonly string AssemblyPath = Assembly.GetExecutingAssembly().Location;
        public static readonly string ExecFilePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

        /// <summary>
        /// 実行ファイル名からプロセス名を取得
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
        public static readonly string WorkDirectoryPath =
            UserInfo.IsSystemAccount ?
                ExecDirectoryPath :
                Path.Combine(Path.GetTempPath(), "EnumRun");

        #endregion
        #region File

        /// <summary>
        /// セッション管理用ファイル
        /// </summary>
        public const string SESSION_FILE = "session.json";

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
        /// Jsonシリアライズ/デシリアライズ用JsonSerialzierOptions
        /// </summary>
        /// <param name="escapeDoubleQuote"></param>
        /// <param name="ignoreReadOnly"></param>
        /// <param name="ignoreNull"></param>
        /// <param name="writeIndented"></param>
        /// <param name="convertEnumCamel"></param>
        /// <returns></returns>
        public static System.Text.Json.JsonSerializerOptions GetJsonSerializerOption(
            bool escapeDoubleQuote,
            bool ignoreReadOnly,
            bool ignoreNull,
            bool writeIndented,
            bool convertEnumCamel)
        {
            var options = convertEnumCamel ?
                new System.Text.Json.JsonSerializerOptions() { Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase) } } :
                new System.Text.Json.JsonSerializerOptions();
            if (escapeDoubleQuote)
            {
                options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            }
            if (ignoreReadOnly)
            {
                options.IgnoreReadOnlyProperties = true;
            }
            if (ignoreNull)
            {
                options.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            }
            if (writeIndented)
            {
                options.WriteIndented = true;
            }

            return options;
        }
    }
}
