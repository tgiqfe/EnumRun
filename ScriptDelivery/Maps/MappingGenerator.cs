using YamlDotNet.Serialization;
using YamlDotNet.RepresentationModel;
using ScriptDelivery.Lib;
using Csv;
using System.IO;
using System.Text;
using ScriptDelivery.Maps.Works;
using ScriptDelivery.Maps.Requires;

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
                        ".yml" => DeserializeYaml(sr),
                        ".yaml" => DeserializeYaml(sr),
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
                    case ".yml":
                    case ".yaml":
                        SerializeYml(list, sw);
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
        #region Yml

        private static ISerializer _serializer = null;

        public static List<Mapping> DeserializeYaml(TextReader tr)
        {
            //  「---」区切りのyamlデータ読み込み
            var list = new List<Mapping>();
            var yaml = new YamlStream();
            yaml.Load(tr);
            foreach (YamlDocument document in yaml.Documents)
            {
                YamlNode node = document.RootNode;
                using (var stream = new MemoryStream())
                using (var writer = new StreamWriter(stream))
                using (var reader = new StreamReader(stream))
                {
                    new YamlStream(new YamlDocument[] { new YamlDocument(node) }).Save(writer);
                    writer.Flush();
                    stream.Position = 0;
                    list.Add(new Deserializer().Deserialize<Mapping>(reader));
                }
            }
            return list;
        }

        private static void SerializeYml(List<Mapping> list, TextWriter tw)
        {
            _serializer ??= new SerializerBuilder().
                WithEmissionPhaseObjectGraphVisitor(x =>
                    new YamlIEnumerableSkipEmptyObjectGraphVisitor(x.InnerVisitor)).
                    Build();
            list.ForEach(x =>
            {
                tw.WriteLine("---");
                tw.WriteLine(_serializer.Serialize(x));
            });
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
                mapping.Work.Downloads[0].Source = line["Source"];
                mapping.Work.Downloads[0].Source = line["Destination"];
                mapping.Work.Downloads[0].Keep = line["Keep"];
                mapping.Work.Downloads[0].UserName = line["User"];
                mapping.Work.Downloads[0].Password = line["Password"];

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
                "Source",
                "Destination",
                "Keep",
                "User",
                "Password",
            };

            Func<Mapping, string[]> toParamArray = (mapping) =>
            {
                return new string[]
                {
                    mapping.Require.GetRequireMode().ToString(),
                    mapping.Require.Rules[0].GetRuleTarget().ToString(),
                    mapping.Require.Rules[0].GetRuleMatch().ToString(),
                    mapping.Require.Rules[0].GetInvert().ToString(),
                    mapping.Require.Rules?.Length > 0 ?
                        string.Join(" ", mapping.Require.Rules[0].Param.Select(x => $"{x.Key}={x.Value}")) : "",
                    mapping.Work.Downloads[0].Source ?? "",
                    mapping.Work.Downloads[0].Destination ?? "",
                    mapping.Work.Downloads[0].GetKeep().ToString(),
                    mapping.Work.Downloads[0].UserName ?? "",
                    mapping.Work.Downloads[0].Password ?? "",
                };
            };

            //CsvWriter.Write(tw, _csvHeader, list.Select(x => x.ToParamArray()));
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
                        if (readLine.StartsWith("Mode:", StringComparison.OrdinalIgnoreCase))
                        {
                            mapping.Require.Mode = readLine.Substring(readLine.IndexOf(":") + 1).Trim();
                            continue;
                        }
                        string[] fields = pattern_delimiter.Split(readLine);
                        var rule = new RequireRule();
                        foreach (string field in fields)
                        {
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
                                        string pairKey = pair.Substring(0, pair.IndexOf("="));
                                        string pairVal = pair.Substring(pair.IndexOf("=") + 1);
                                        rule.Param[pairKey] = pairVal;
                                    }
                                    break;
                            }
                        }
                        mapping.Require.Rules ??= new RequireRule[0];
                        mapping.Require.Rules = mapping.Require.Rules.Concat(new RequireRule[] { rule }).ToArray();
                    }
                    else
                    {
                        //  Work取得
                        string[] fields = pattern_delimiter.Split(readLine);
                        var download = new Download();
                        foreach (string field in fields)
                        {

                            string key = field.Substring(0, field.IndexOf(":")).Trim().ToLower();
                            string val = field.Substring(field.IndexOf(":") + 1).Trim();
                            switch (key.ToLower())
                            {
                                case "source":
                                    download.Source = val;
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
                        mapping.Work.Downloads ??= new Download[0];
                        mapping.Work.Downloads = mapping.Work.Downloads.Concat(new Download[] { download }).ToArray();
                    }
                }
            }
            list.Add(mapping);

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
                    sb.Append("  Source: " + download.Source);
                    sb.Append(", Destination: " + download.Destination);
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
            }
        }

        #endregion
    }
}
