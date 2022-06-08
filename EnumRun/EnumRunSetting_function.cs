using EnumRun.Lib;
using EnumRun.Logs.ProcessLog;
using Hnx8.ReadJEnc;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EnumRun
{
    internal partial class EnumRunSetting
    {
        /// <summary>
        /// 初期値をセット
        /// </summary>
        public void SetDefault()
        {
            this.FilesPath = Path.Combine(Item.WorkDirectoryPath, "Files");
            this.LogsPath = Path.Combine(Item.WorkDirectoryPath, "Logs");
            this.OutputPath = Path.Combine(Item.WorkDirectoryPath, "Output");
            this.RestTime = 60;
            this.DefaultOutput = false;
            this.RetentionPeriod = 0;
            this.MinLogLevel = "info";
            this.Ranges = new ParamRanges()
            {
                { "StartupScript", "0-9" },
                { "LogonScript", "11-29" },
                { "LogoffScript", "81-89" },
                { "ShutdownScript", "91-99" },
            };
            this.Logstash = new ParamLogstash()
            {
                Server = null
            };
            this.Syslog = new ParamSyslog()
            {
                Server = null,
                Facility = "user",
                Format = "RFC3164",
                SslEncrypt = false,
                SslTimeout = 3000,
                SslCertFile = null,
                SslCertPassword = null,
                SslCertFriendryName = null,
                SslIgnoreCheck = false
            };
            this.ScriptDelivery = new ParamScriptDelivery()
            {
                Server = new string[] { "http://localhost:5000" },
                Process = "StartupScript",
                TrashPath = null,
                LogTransport = true,
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
            if (File.Exists(jsonFilePath))
            {
                return DeserializeJson(jsonFilePath);
            }

            string textFilePath = TargetDirectory.GetFile(Item.CONFIG_TXT);
            if (File.Exists(textFilePath))
            {
                return DeserializeText(textFilePath);
            }

            return DeserializeJson(jsonFilePath);
        }

        /// <summary>
        /// Jsonファイルを読み込んでデシリアライズ
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
                            Item.GetJsonSerializerOption(
                                escapeDoubleQuote: false,
                                ignoreReadOnly: true,
                                ignoreNull: true,
                                writeIndented: false,
                                convertEnumCamel: true));
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
        /// Textファイルを読み込んでデシリアライズ
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
                using (var sr = new StringReader(fr.Text))
                {
                    string readLine = "";
                    var lineList = new List<string>();
                    while ((readLine = sr.ReadLine()) != null) { lineList.Add(readLine); }

                    int index = 0;
                    setting = GetProperty(new EnumRunSetting(), lineList, ref index);
                }
            }

            return setting;
        }

        private static T GetProperty<T>(T obj, List<string> list, ref int index, bool isRoot = true) where T : class
        {
            Regex pat_indent = new Regex(@"^(\s{2})+");
            PropertyInfo[] props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            for (int i = index; i < list.Count; i++, index++)
            {
                if (string.IsNullOrEmpty(list[i])) { continue; }
                if (isRoot || pat_indent.IsMatch(list[i]))
                {
                    string key = list[i].Substring(0, list[i].IndexOf(":")).Trim();
                    string val = list[i].Substring(list[i].IndexOf(":") + 1).Trim();
                    PropertyInfo prop = props.FirstOrDefault(x => x.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
                    if (prop != null)
                    {
                        Type type = prop.PropertyType;
                        if (type == typeof(string))
                        {
                            prop.SetValue(obj, val);
                        }
                        else if (type == typeof(int?))
                        {
                            prop.SetValue(obj, int.TryParse(val, out int tempInt) ? tempInt : null);
                        }
                        else if (type == typeof(bool?))
                        {
                            prop.SetValue(obj, !BooleanCandidate.IsNullableFalse(val));
                        }
                        else if (type == typeof(string[]))
                        {
                            prop.SetValue(obj, val.Split(',').Select(x => x.Trim()).ToArray());
                        }
                        else if (type == typeof(ParamRanges))
                        {
                            var ranges = new ParamRanges();
                            for (i++; i < list.Count; i++)
                            {
                                if (pat_indent.IsMatch(list[i]))
                                {
                                    key = list[i].Substring(0, list[i].IndexOf(":")).Trim();
                                    val = list[i].Substring(list[i].IndexOf(":") + 1).Trim();
                                    ranges[key] = val;
                                }
                                else
                                {
                                    i--;
                                    index = i;
                                    break;
                                }
                            }
                            (obj as EnumRunSetting).Ranges = ranges;
                        }
                        else if (type == typeof(ParamLogstash))
                        {
                            index++;
                            (obj as EnumRunSetting).Logstash = GetProperty(new ParamLogstash(), list, ref index, false);
                            i = --index;
                        }
                        else if (type == typeof(ParamSyslog))
                        {
                            index++;
                            (obj as EnumRunSetting).Syslog = GetProperty(new ParamSyslog(), list, ref index, false);
                            i = --index;
                        }
                        else if (type == typeof(ParamScriptDelivery))
                        {
                            index++;
                            (obj as EnumRunSetting).ScriptDelivery = GetProperty(new ParamScriptDelivery(), list, ref index, false);
                            i = --index;
                        }
                    }
                }
                else { break; }
            }
            return obj;
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
                string json = JsonSerializer.Serialize(this, Item.GetJsonSerializerOption(
                    escapeDoubleQuote: true,
                    ignoreReadOnly: true,
                    ignoreNull: true,
                    writeIndented: true,
                    convertEnumCamel: true));
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
                if (this.Ranges != null)
                {
                    sw.WriteLine("Ranges:");
                    foreach (var pair in this.Ranges)
                    {
                        sw.WriteLine($"  {pair.Key}: {pair.Value}");
                    }
                }
                if (this.Logstash != null)
                {
                    sw.WriteLine("Logstash:");
                    sw.WriteLine($"  Server: {this.Logstash.Server}");
                }
                if (this.Syslog != null)
                {
                    sw.WriteLine("Syslog:");
                    sw.WriteLine($"  Server: {this.Syslog.Server}");
                    sw.WriteLine($"  Facility: {this.Syslog.Facility}");
                    sw.WriteLine($"  Format: {this.Syslog.Format}");
                    sw.WriteLine($"  SslEncrypt: {this.Syslog.SslEncrypt}");
                    sw.WriteLine($"  SslTimeout: {this.Syslog.SslTimeout}");
                    sw.WriteLine($"  SslCertFile: {this.Syslog.SslCertFile}");
                    sw.WriteLine($"  SslCertPassword: {this.Syslog.SslCertPassword}");
                    sw.WriteLine($"  SslCertFriendryName: {this.Syslog.SslCertFriendryName}");
                    sw.WriteLine($"  SslIgnoreCheck: {this.Syslog.SslIgnoreCheck}");
                }
                if (this.ScriptDelivery != null)
                {
                    sw.WriteLine("ScriptDelivery:");
                    sw.WriteLine($"  Server: {string.Join(", ", this.ScriptDelivery.Server)}");
                    sw.WriteLine($"  Process: {this.ScriptDelivery.Process}");
                    sw.WriteLine($"  TrashPath: {this.ScriptDelivery.TrashPath}");
                    sw.WriteLine($"  LogTransport: {this.ScriptDelivery.LogTransport}");
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

        #region Expand env

        public string GetFilesPath()
        {
            return string.IsNullOrEmpty(this.FilesPath) ?
                Path.Combine(Item.WorkDirectoryPath, "Files") :
                ExpandEnvironment(this.FilesPath);
        }

        public string GetLogsPath()
        {
            return string.IsNullOrEmpty(this.LogsPath) ?
                Path.Combine(Item.WorkDirectoryPath, "Logs") :
                ExpandEnvironment(this.LogsPath);
        }

        public string GetOutputPath()
        {
            return string.IsNullOrEmpty(this.OutputPath) ?
                Path.Combine(Item.WorkDirectoryPath, "Output") :
                ExpandEnvironment(this.OutputPath);
        }

        private string ExpandEnvironment(string text)
        {
            for (int i = 0; i < 5 && text.Contains("%"); i++)
            {

                text = Environment.ExpandEnvironmentVariables(text);
            }
            return text;
        }

        #endregion
    }
}
