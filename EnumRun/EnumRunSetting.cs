using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumRun.Lib;
using System.Text.Json;
using System.IO;
using System.Text.Json.Serialization;

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

        /// <summary>
        /// デシリアライズ
        /// </summary>
        /// <returns></returns>
        public static EnumRunSetting Deserialize()
        {
            string[] _targetCandidate = new string[]
            {
                Path.Combine(Item.WorkDirectory, Item.CONFIG_JSON),
                Path.Combine(Item.AssemblyDirectory, Item.CONFIG_JSON),
            };
            string configPath = _targetCandidate.
                FirstOrDefault(x => File.Exists(x));

            EnumRunSetting setting = null;
            if (configPath != null)
            {
                try
                {
                    using (var sr = new StreamReader(configPath, Encoding.UTF8))
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
    }
}
