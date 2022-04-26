using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumRun.Lib;
using System.Text.Json;
using System.IO;
using System.Text.Json.Serialization;
using Hnx8.ReadJEnc;
using System.Text.RegularExpressions;

namespace EnumRun
{
    internal class EnumRunSetting
    {
        public string FilesPath { get; set; }
        public string LogsPath { get; set; }
        public string OutputPath { get; set; }
        public bool RunOnce { get; set; }
        public ProcessRange Ranges { get; set; }

        /// <summary>
        /// 初期値をセット
        /// </summary>
        public void SetDefault()
        {
            this.FilesPath = Path.Combine(Item.WorkDirectory, "Files");
            this.LogsPath = Path.Combine(Item.WorkDirectory, "Logs");
            this.OutputPath = Path.Combine(Item.WorkDirectory, "Output");
            this.RunOnce = false;
            this.Ranges = new ProcessRange()
            {
                { "StartupScript", "0-9" },
                { "ShutdownScript", "11-29" },
                { "LogonScript", "81-89" },
                { "LogoffScript", "91-99" },
            };
        }

        #region Json serialize

        /// <summary>
        /// デシリアライズ
        /// </summary>
        /// <returns></returns>
        public static EnumRunSetting Deserialize()
        {
            string jsonConfigPath = new string[]
            {
                Path.Combine(Item.WorkDirectory, Item.CONFIG_JSON),
                Path.Combine(Item.AssemblyDirectory, Item.CONFIG_JSON),
            }.FirstOrDefault(x => File.Exists(x));
            if(jsonConfigPath != null)
            {
                return DeserializeJson(jsonConfigPath);
            }

            string textConfigPath = new string[]
            {
                Path.Combine(Item.WorkDirectory, Item.CONFIG_TXT),
                Path.Combine(Item.AssemblyDirectory, Item.CONFIG_TXT),
            }.FirstOrDefault(x => File.Exists(x));
            if(textConfigPath != null)
            {
                return DeserializeText(textConfigPath);
            }

            return null;
        }

        public static EnumRunSetting DeserializeJson(string filePath)
        {
            EnumRunSetting setting = null;
            if (filePath != null)
            {
                try
                {
                    using (var sr = new StreamReader(filePath, Encoding.UTF8))
                    {
                        setting = JsonSerializer.Deserialize<EnumRunSetting>(sr.ReadToEnd());
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
                        string key = readLine.Substring(0, readLine.IndexOf(":"));
                        string val = readLine.Substring(readLine.IndexOf(":") + 1);
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
                            case "runonce":
                                setting.RunOnce = !BooleanCandidate.IsFalse(val);
                                break;
                            case "ranges":
                            case "range":
                                setting.Ranges = new ProcessRange();
                                string readLine2 = "";
                                Regex pattern_indent = new Regex(@"^(\s{2})+");
                                while ((readLine2 = sr.ReadLine()) != null)
                                {
                                    if (!pattern_indent.IsMatch(readLine2))
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



        /// <summary>
        /// シリアライズ
        /// </summary>
        /// <param name="filePath"></param>
        public void Serialize(string filePath)
        {
            ParentDirectory.Create(filePath);
            using (var sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                string json = JsonSerializer.Serialize(this,
                    new JsonSerializerOptions()
                    {
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        WriteIndented = true
                    });
                sw.WriteLine(json);
            }
        }

        #endregion
        #region Text serialize

        
        public static void SerializeText(string filePath)
        {

        }

        #endregion
    }
}
