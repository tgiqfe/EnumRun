using ScriptDelivery.Lib;
using Csv;
using System.IO;
using System.Text;
using ScriptDelivery.Maps.Works;
using ScriptDelivery.Maps.Requires;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScriptDelivery.Maps
{
    internal class MappingGenerator
    {
        #region Deserialize/Serialize

        /// <summary>
        /// 指定したファイルから読み込んでデシリアライズ
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static List<Mapping> Deserialize(string filePath)
        {
            List<Mapping> list = null;
            string fileName = Path.GetFileName(filePath);

            if (File.Exists(filePath))
            {
                using (var sr = new StreamReader(filePath, Encoding.UTF8))
                {
                    string extention = Path.GetExtension(filePath);
                    list = extention switch
                    {
                        ".json" => DeserializeJson(sr),
                        ".csv" => DeserializeCsv(sr),
                        ".txt" => DeserializeTxt(sr),
                        _ => null,
                    };
                }
                list.ForEach(x => x.Name = Path.GetFileName(filePath));
            }
            else if (Directory.Exists(filePath))
            {
                foreach (string childPath in Directory.GetFiles(filePath))
                {
                    list.AddRange(Deserialize(childPath));
                }
            }

            return list;
        }

        /// <summary>
        /// シリアライズして指定したファイルへ書き込み
        /// </summary>
        /// <param name="list"></param>
        /// <param name="filePath"></param>
        public static void Serialize(List<Mapping> list, string filePath)
        {
            using (var sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                string extention = Path.GetExtension(filePath);
                switch (extention)
                {
                    case ".json":
                        SerializeJson(list, sw);
                        break;
                    case ".csv":
                        SerializeCsv(list, sw);
                        break;
                    case ".txt":
                        SerializeTxt(list, sw);
                        break;
                }
            }
        }

        #endregion
        #region Json

        public static List<Mapping> DeserializeJson(TextReader tr)
        {
            List<Mapping> list = JsonSerializer.Deserialize<List<Mapping>>(
                tr.ReadToEnd(),
                new JsonSerializerOptions()
                {
                    //Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    //IgnoreReadOnlyProperties = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    //WriteIndented = true,
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                });
            return list ?? new List<Mapping>();
        }

        public static void SerializeJson(List<Mapping> list, TextWriter tw)
        {
            string json = JsonSerializer.Serialize(list,
                new JsonSerializerOptions()
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    IgnoreReadOnlyProperties = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true,
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                });
            tw.WriteLine(json);
        }

        #endregion
        #region Csv

        private static string[] _csvHeader = null;

        public static List<Mapping> DeserializeCsv(TextReader tr)
        {
            var separator = new System.Text.RegularExpressions.Regex(@"\s(?=[^\s]+=)");

            var list = new List<Mapping>();
            foreach (var line in CsvReader.Read(tr))
            {
                var mapping = new Mapping();
                mapping.Require = new Require();
                mapping.Require.Mode = line["Mode"];

                mapping.Require.Rules = new RequireRule[1] { new RequireRule() };
                mapping.Require.Rules[0].Target = line["Target"];
                mapping.Require.Rules[0].Match = line["Match"];
                mapping.Require.Rules[0].Invert = line["Invert"];

                var dictionary = new Dictionary<string, string>();
                separator.Split(line["Param"]).
                    ToList().
                    ForEach(x =>
                    {
                        if (x.Contains("="))
                        {
                            string[] fields = x.Split('=');
                            dictionary[fields[0]] = fields[1];
                        }
                    });
                mapping.Require.Rules[0].Param = dictionary;

                mapping.Work = new Work();
                mapping.Work.Downloads = new Download[1] { new Download() };
                mapping.Work.Downloads[0].Path = line["Path"];
                mapping.Work.Downloads[0].Destination = line["Destination"];
                mapping.Work.Downloads[0].Keep = line["Keep"];
                mapping.Work.Downloads[0].UserName = line["User"];
                mapping.Work.Downloads[0].Password = line["Password"];
                mapping.Work.Delete.DeleteTarget = line["DeleteTarget"].
                    Split(System.IO.Path.PathSeparator).Select(x => x.Trim()).ToArray();
                mapping.Work.Delete.DeleteExclude = line["DeleteExclude"].
                    Split(System.IO.Path.PathSeparator).Select(x => x.Trim()).ToArray();

                list.Add(mapping);
            }
            return list;
        }

        private static void SerializeCsv(List<Mapping> list, TextWriter tw)
        {
            _csvHeader ??= new string[]
            {
                "Mode",
                "Target",
                "Match",
                "Invert",
                "Param",
                "Path",
                "Destination",
                "Keep",
                "User",
                "Password",
                "DeleteTarget",
                "DeleteExclude"
            };

            Func<Mapping, string[]> toParamArray = (mapping) =>
            {
                string deleteTarget = "";
                string deleteExclude = "";
                if (mapping.Work.Delete != null)
                {
                    deleteTarget = mapping.Work.Delete.DeleteTarget?.Length > 0 ?
                        string.Join(System.IO.Path.PathSeparator, mapping.Work.Delete.DeleteTarget) : "";
                    deleteExclude = mapping.Work.Delete.DeleteExclude?.Length > 0 ?
                        String.Join(System.IO.Path.PathSeparator, mapping.Work.Delete.DeleteExclude) : "";
                }

                return new string[]
                {
                    mapping.Require.GetRequireMode().ToString(),
                    mapping.Require.Rules[0].GetRuleTarget().ToString(),
                    mapping.Require.Rules[0].GetRuleMatch().ToString(),
                    mapping.Require.Rules[0].GetInvert().ToString(),
                    mapping.Require.Rules?.Length > 0 ?
                        string.Join(" ", mapping.Require.Rules[0].Param.Select(x => $"{x.Key}={x.Value}")) : "",
                    mapping.Work.Downloads[0].Path ?? "",
                    mapping.Work.Downloads[0].Destination ?? "",
                    mapping.Work.Downloads[0].GetKeep().ToString(),
                    mapping.Work.Downloads[0].UserName ?? "",
                    mapping.Work.Downloads[0].Password ?? "",
                    deleteTarget,
                    deleteExclude,
                };
            };

            CsvWriter.Write(tw, _csvHeader, list.Select(x => toParamArray(x)));
        }

        #endregion
        #region Txt

        private static List<Mapping> DeserializeTxt(TextReader tr)
        {
            System.Text.RegularExpressions.Regex pattern_comment =
                new System.Text.RegularExpressions.Regex(@"(?<=^[^'""]*)\s*#.*$|(?<=^([^'""]*([^'""]*['""]){2})*)\s*#.*$");
            System.Text.RegularExpressions.Regex pattern_delimiter =
                new System.Text.RegularExpressions.Regex(@"(?<=^[^\{\}]+),|(?<=(\{.*\})+[^\{]*),");

            var list = new List<Mapping>();
            Mapping mapping = null;
            bool? isRequire = null;
            string readLine = "";
            while ((readLine = tr.ReadLine()?.Trim()) != null)
            {
                if (readLine.StartsWith("---"))
                {
                    if (mapping != null)
                    {
                        list.Add(mapping);
                    }
                    mapping = new Mapping();
                    continue;
                }
                if (pattern_comment.IsMatch(readLine))
                {
                    readLine = pattern_comment.Replace(readLine, "");
                }
                if (readLine == "")
                {
                    continue;
                }
                if (readLine.StartsWith("Require:", StringComparison.OrdinalIgnoreCase))
                {
                    isRequire = true;
                    mapping.Require = new Require();
                    continue;
                }
                else if (readLine.StartsWith("Work:", StringComparison.OrdinalIgnoreCase))
                {
                    isRequire = false;
                    mapping.Work = new Work();
                    continue;
                }
                if (isRequire != null)
                {
                    if (isRequire ?? false)
                    {
                        //  Require取得
                        //  Mode
                        if (readLine.StartsWith("Mode:", StringComparison.OrdinalIgnoreCase))
                        {
                            mapping.Require.Mode = readLine.Substring(readLine.IndexOf(":") + 1).Trim();
                            continue;
                        }

                        //  RequireRule
                        string[] fields = pattern_delimiter.Split(readLine);
                        var rule = new RequireRule();
                        foreach (string field in fields)
                        {
                            if (!field.Contains(":")) { continue; }

                            string key = field.Substring(0, field.IndexOf(":")).Trim();
                            string val = field.Substring(field.IndexOf(":") + 1).Trim();
                            switch (key.ToLower())
                            {
                                case "target":
                                    rule.Target = val;
                                    break;
                                case "match":
                                    rule.Match = val;
                                    break;
                                case "invert":
                                    rule.Invert = val;
                                    break;
                                case "param":
                                    rule.Param = new Dictionary<string, string>();
                                    foreach (string pair in val.TrimStart('{').TrimEnd('}').Split(","))
                                    {
                                        string pairKey = pair.Substring(0, pair.IndexOf("=")).Trim();
                                        string pairVal = pair.Substring(pair.IndexOf("=") + 1).Trim();
                                        rule.Param[pairKey] = pairVal;
                                    }
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(rule.Target))
                        {
                            mapping.Require.Rules ??= new RequireRule[0];
                            mapping.Require.Rules = mapping.Require.Rules.Concat(new RequireRule[] { rule }).ToArray();
                        }
                    }
                    else
                    {
                        //  Work取得
                        //  Delete
                        if (readLine.StartsWith("Delete:", StringComparison.OrdinalIgnoreCase))
                        {
                            string deleteVal = readLine.Substring(readLine.IndexOf(":") + 1).
                                Trim().TrimStart('{').TrimEnd('}');
                            string[] delFields = deleteVal.Split(',').Select(x => x.Trim()).ToArray();
                            mapping.Work.Delete = new DeleteFile();
                            foreach (string field in delFields)
                            {
                                if (string.IsNullOrEmpty(field)) { continue; }

                                string key = field.Substring(0, field.IndexOf("=")).Trim().ToLower();
                                string val = field.Substring(field.IndexOf("=") + 1).Trim();
                                switch (key.ToLower())
                                {
                                    case "target":
                                        mapping.Work.Delete.DeleteTarget = val.Split(System.IO.Path.PathSeparator);
                                        break;
                                    case "exclude":
                                        mapping.Work.Delete.DeleteExclude = val.Split(System.IO.Path.PathSeparator);
                                        break;
                                }
                            }
                        }

                        //  Download
                        string[] fields = pattern_delimiter.Split(readLine);
                        var download = new Download();
                        foreach (string field in fields)
                        {
                            if (string.IsNullOrEmpty(field)) { continue; }

                            string key = field.Substring(0, field.IndexOf(":")).Trim().ToLower();
                            string val = field.Substring(field.IndexOf(":") + 1).Trim();
                            switch (key.ToLower())
                            {
                                case "path":
                                    download.Path = val;
                                    break;
                                case "destination":
                                    download.Destination = val;
                                    break;
                                case "keep":
                                    download.Keep = val;
                                    break;
                                case "user":
                                    download.UserName = val;
                                    break;
                                case "password":
                                    download.Password = val;
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(download.Path))
                        {
                            mapping.Work.Downloads ??= new Download[0];
                            mapping.Work.Downloads = mapping.Work.Downloads.Concat(new Download[] { download }).ToArray();
                        }
                    }
                }
            }
            if (mapping != null)
            {
                list.Add(mapping);
            }

            return list;
        }

        private static void SerializeTxt(List<Mapping> list, TextWriter tw)
        {
            foreach (var mapping in list)
            {
                tw.WriteLine("---");
                tw.WriteLine("Require:");
                tw.WriteLine("  Mode: {0}", mapping.Require.GetRequireMode());
                foreach (var rule in mapping.Require.Rules)
                {
                    var sb = new StringBuilder();
                    sb.Append("  Target: " + rule.Target);
                    if (rule.GetRuleMatch() != RuleMatch.Equal)
                    {
                        sb.Append($", Match: {rule.GetRuleMatch()}");
                    }
                    if (rule.GetInvert())
                    {
                        sb.Append(", Invert: true");
                    }
                    if (rule.Param?.Count > 0)
                    {
                        string paramText = string.Join(", ",
                            rule.Param.Select(x => $"{x.Key}={x.Value}"));
                        sb.Append($", Param: {{ {paramText} }}");
                    }
                    tw.WriteLine(sb.ToString());
                }
                tw.WriteLine("Work:");
                foreach (var download in mapping.Work.Downloads)
                {
                    var sb = new StringBuilder();
                    sb.Append("  Path: " + download.Path);
                    if (!string.IsNullOrEmpty(download.Destination))
                    {
                        sb.Append(", Destination: " + download.Destination);
                    }
                    if (download.GetKeep())
                    {
                        sb.Append(", Keep: true");
                    }
                    if (!string.IsNullOrEmpty(download.UserName))
                    {
                        sb.Append(", User: " + download.UserName);
                    }
                    if (!string.IsNullOrEmpty(download.Password))
                    {
                        sb.Append(", Password: " + download.Password);
                    }
                    tw.WriteLine(sb.ToString());
                }
                if (mapping.Work.Delete != null)
                {
                    var sb = new StringBuilder();
                    sb.Append("  Delete: {");
                    if (mapping.Work.Delete.DeleteTarget?.Length > 0)
                    {
                        sb.Append(" Target=" + string.Join(System.IO.Path.PathSeparator, mapping.Work.Delete.DeleteTarget));
                    }
                    if (mapping.Work.Delete.DeleteExclude?.Length > 0)
                    {
                        sb.Append(", Exclude=" + string.Join(System.IO.Path.PathSeparator, mapping.Work.Delete.DeleteExclude));
                    }
                    sb.Append(" }");
                    tw.WriteLine(sb.ToString());
                }
            }
        }

        #endregion
    }
}
