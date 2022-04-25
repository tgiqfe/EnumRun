using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;
using System.IO;

namespace EnumRun.Lib
{
    internal class LanguageCollection : List<Language>
    {
        /// <summary>
        /// デシリアライズ用静的メソッド
        /// </summary>
        /// <returns></returns>
        public static LanguageCollection Deserialize()
        {
            string[] _targetCandidate = new string[]
            {
                Path.Combine(Item.WorkDirectory),
                Path.Combine(Item.AssemblyDirectory),
            };
            string configPath = _targetCandidate.
                Select(x => Path.Combine(x, Item.LANG_JSON)).
                FirstOrDefault(x => File.Exists(x));

            var collection = new LanguageCollection();
            collection.Load(configPath);

            return collection;
        }

        #region Load/Save

        public void Load(string path)
        {
            List<Language> list = null;
            try
            {
                using (var sr = new StreamReader(path, Encoding.UTF8))
                {
                    list = JsonSerializer.Deserialize<List<Language>>(sr.ReadToEnd());
                }
            }
            catch { }
            if (list == null)
            {
                list = DefaultLanguageSetting.Create();
            }

            this.Clear();
            this.AddRange(list);
            //this.Save(path);
        }

        public void Save(string path)
        {
            ParentDirectory.Create(path);

            try
            {
                using (var sw = new StreamWriter(path, false, Encoding.UTF8))
                {
                    string json = JsonSerializer.Serialize(this, new JsonSerializerOptions()
                    {
                        WriteIndented = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    });
                    sw.WriteLine(json);
                }
            }
            catch { }
        }

        #endregion

        public Language GetLanguage(string filePath)
        {
            if (File.Exists(filePath))
            {
                string extension = Path.GetExtension(filePath);
                return this.FirstOrDefault(x =>
                    x.Extensions.Any(y =>
                        y.Equals(extension, StringComparison.OrdinalIgnoreCase)));
            }
            return null;
        }

        public Process GetProcess(string filePath)
        {
            if (File.Exists(filePath))
            {
                string extension = Path.GetExtension(filePath);
                Language lang = this.FirstOrDefault(x =>
                    x.Extensions.Any(y =>
                        y.Equals(extension, StringComparison.OrdinalIgnoreCase)));
                return lang == null ?
                    null :
                    lang.GetProcess(filePath, "");
            }
            return null;
        }
    }
}
