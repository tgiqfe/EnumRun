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
        /// <summary>
        /// 初期値をセット
        /// </summary>
        public void SetDefault()
        {
            this.FilesPath = Path.Combine(Item.WorkDirectory, "Files");
            this.LogsPath = Path.Combine(Item.WorkDirectory, "Logs");
            this.OutputPath = Path.Combine(Item.WorkDirectory, "Output");
            this.RestTime = 60;
            this.DefaultOutput = false;
            this.RetentionPeriod = 0;
            this.MinLogLevel = LogLevel.Info;
            this.LogstashServer = null;
            this.SyslogServer = null;
            this.SyslogFacility = "user";
            this.SyslogFormat = Log.Syslog.SyslogFormat.RFC3164;
            this.SyslogSslEncrypt = false;
            this.SyslogSslTimeout = 3000;
            this.SyslogSslCertFile = null;
            this.SyslogSslCertPassword = null;
            this.SyslogSslCertFriendryName = "syslog";
            this.SyslogSslIgnoreCheck = false;
            this.Ranges = new ProcessRanges()
            {
                { "StartupScript", "0-9" },
                { "ShutdownScript", "11-29" },
                { "LogonScript", "81-89" },
                { "LogoffScript", "91-99" },
            };
        }

        #region Deserialize

        /// <summary>
        /// デシリアライズ
        /// </summary>
        /// <returns></returns>
        public static EnumRunSetting Deserialize()
        {
            string jsonFilePath = TargetDirectory.GetFile(Item.CONFIG_JSON);
            string textFilePath = TargetDirectory.GetFile(Item.CONFIG_TXT);

            if (File.Exists(jsonFilePath))
            {
                return DeserializeJson(jsonFilePath);
            }
            else if (File.Exists(textFilePath))
            {
                return DeserializeText(textFilePath);
            }
            return DeserializeJson(jsonFilePath);
        }

        /// <summary>
        /// Jsonファイルからシリアライズ
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static EnumRunSetting DeserializeJson(string filePath)
        {
            EnumRunSetting setting = null;
            if (filePath != null)
            {
                try
                {
                    using (var sr = new StreamReader(filePath, Encoding.UTF8))
                    {
                        setting = JsonSerializer.Deserialize<EnumRunSetting>(
                            sr.ReadToEnd(),
                            new JsonSerializerOptions()
                            {
                                //Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                                //IgnoreReadOnlyProperties = true,
                                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                                //WriteIndented = true,
                                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                            });
                    }
                }
                catch { }
            }
            if (setting == null)
            {
                setting = new EnumRunSetting();
                setting.SetDefault();
            }

            return setting;
        }

        /// <summary>
        /// Textファイルからデシリアライズ
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static EnumRunSetting DeserializeText(string filePath)
        {
            var setting = new EnumRunSetting();

            var info = new FileInfo(filePath);
            using (FileReader fr = new FileReader(info))
            {
                fr.Read(info);
                using (StringReader sr = new StringReader(fr.Text))
                {
                    string readLine = "";
                    while ((readLine = sr.ReadLine()) != null)
                    {
                        string key = readLine.Substring(0, readLine.IndexOf(":")).Trim();
                        string val = readLine.Substring(readLine.IndexOf(":") + 1).Trim();
                        switch (key.ToLower())
                        {
                            case "filespath":
                            case "filepath":
                                setting.FilesPath = val;
                                break;
                            case "logspath":
                            case "logpath":
                                setting.LogsPath = val;
                                break;
                            case "outputspath":
                            case "outputpath":
                                setting.OutputPath = val;
                                break;
                            case "resttime":
                                setting.RestTime = int.TryParse(val, out int num1) ? num1 : 0;
                                break;
                            case "defaultoutput":
                                setting.DefaultOutput = !BooleanCandidate.IsFalse(val);
                                break;
                            case "retentionperiod":
                                setting.RetentionPeriod = int.TryParse(val, out int num2) ? num2 : 0;
                                break;
                            case "minloglevel":
                                setting.MinLogLevel = Enum.TryParse(val, ignoreCase: true, out LogLevel level) ? level : LogLevel.Info;
                                break;
                            case "logstashserver":
                                setting.LogstashServer = val;
                                break;
                            case "ranges":
                            case "range":
                                setting.Ranges = new ProcessRanges();
                                string readLine2 = "";
                                Regex pat_indent = new Regex(@"^(\s{2})+");
                                while ((readLine2 = sr.ReadLine()) != null)
                                {
                                    if (!pat_indent.IsMatch(readLine2))
                                    {
                                        break;
                                    }
                                    string key2 = readLine2.Substring(0, readLine2.IndexOf(":"));
                                    string val2 = readLine2.Substring(readLine2.IndexOf(":") + 1);
                                    setting.Ranges[key2] = val2;
                                }
                                break;
                        }
                    }
                }
            }

            return setting;
        }

        #endregion
        #region Serialize

        /// <summary>
        /// シリアライズ
        /// </summary>
        /// <param name="filePath"></param>
        public void Serialize(string filePath)
        {
            TargetDirectory.CreateParent(filePath);
            switch (Path.GetExtension(filePath))
            {
                case ".json":
                    SerializeJson(filePath);
                    break;
                case ".txt":
                    SerializeText(filePath);
                    break;
            }
        }

        /// <summary>
        /// Jsonファイルへシリアライズ
        /// </summary>
        /// <param name="filePath"></param>
        public void SerializeJson(string filePath)
        {
            using (var sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                string json = JsonSerializer.Serialize(this,
                    new JsonSerializerOptions()
                    {
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        IgnoreReadOnlyProperties = true,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                        WriteIndented = true,
                        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                    });
                sw.WriteLine(json);
            }
        }

        /// <summary>
        /// Textファイルへシリアライズ
        /// </summary>
        /// <param name="filePath"></param>
        public void SerializeText(string filePath)
        {
            //  BOM無しUTF-8は、new System.Text.UTF8Encoding(false)でも可。
            //  今回は、デシリアライズ時の自動エンコードチェックの為に使用したReadJEncを使用。
            using (var sw = new StreamWriter(filePath, false, FileType.UTF8N.GetEncoding()))
            {
                sw.WriteLine($"FilesPath: {this.FilesPath}");
                sw.WriteLine($"LogsPath: {this.LogsPath}");
                sw.WriteLine($"OutputPath: {this.OutputPath}");
                sw.WriteLine($"RestTime: {this.RestTime}");
                sw.WriteLine($"DefaultOutput: {this.DefaultOutput}");
                sw.WriteLine($"RetentionPeriod: {this.RetentionPeriod}");
                sw.WriteLine($"MinLogLevel: {this.MinLogLevel}");
                sw.WriteLine($"LogstashServer: {this.LogstashServer}");
                sw.WriteLine("Ranges:");
                foreach (var pair in this.Ranges)
                {
                    sw.WriteLine($"  {pair.Key}: {pair.Value}");
                }
            }
        }

        #endregion

        /// <summary>
        /// ログ出力用メソッド
        /// </summary>
        /// <returns></returns>
        public string ToLog()
        {
            var props = this.GetType().GetProperties(
                BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            return string.Join(", ",
                props.Select(x => x.Name + " => " + x.GetValue(this)?.ToString()));
        }
    }
}
