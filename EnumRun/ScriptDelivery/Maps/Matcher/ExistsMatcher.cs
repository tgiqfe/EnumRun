using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumRun.Lib;
using System.IO;
using Microsoft.Win32;
using ScriptDelivery.Maps;
using ScriptDelivery.Maps.Requires;
using ScriptDelivery;
using EnumRun.Logs;

namespace EnumRun.ScriptDelivery.Maps.Matcher
{
    internal class ExistsMatcher : MatcherBase
    {
        [MatcherParameter(Mandatory = true, Expand = true), Keys("Path", "RegistryKey")]
        public string Path { get; set; }

        [MatcherParameter, Keys("Name", "RegistryName")]
        public string Name { get; set; }

        public override bool IsMatch(RuleMatch ruleMatch)
        {
            bool ret = ruleMatch switch
            {
                RuleMatch.File => FileMatch(),
                RuleMatch.Directory => DirectoryMatch(),
                RuleMatch.Registry => RegistryMatch(),
                _ => false,
            };

            _logger.Write(ret ? LogLevel.Debug : LogLevel.Attention, $"MatchType => {ruleMatch}, Match => {ret}");

            return ret;
        }

        #region Match methods

        /// <summary>
        /// ファイルの有無チェック
        /// </summary>
        /// <returns></returns>
        private bool FileMatch()
        {
            if (this.Path.Contains("*"))
            {
                return SearchWildFile(this.Path, false);
            }
            return File.Exists(this.Path);
        }

        /// <summary>
        /// ディレクトリの有無チェック
        /// </summary>
        /// <returns></returns>
        private bool DirectoryMatch()
        {
            if (this.Path.Contains("*"))
            {
                return SearchWildFile(this.Path, isDirectory: true);
            }
            return Directory.Exists(this.Path);
        }

        /// <summary>
        /// レジストリキー or レジストリ値の有無チェック
        /// </summary>
        /// <returns></returns>
        private bool RegistryMatch()
        {
            if (this.Path.Contains("*") || this.Name.Contains("*"))
            {
                return SearchWildRegistry(Path, Name);
            }
            return RegistryControl.Exists(Path, Name);
        }

        #endregion
        #region Search method

        /// <summary>
        /// ワイルドカードを含むパスからファイル/ディレクトリの有無チェック
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isDirectory"></param>
        /// <returns></returns>
        private bool SearchWildFile(string path, bool isDirectory)
        {
            string parentPath = System.IO.Path.GetDirectoryName(path.TrimEnd('\\'));
            var pattern = path.TrimEnd('\\').GetWildcardPattern();
            var list = new List<string>();

            Action<string> recurse = null;
            recurse = (dirPath) =>
            {
                if (!isDirectory)
                {
                    foreach (string subFilePath in Directory.GetFiles(dirPath))
                    {
                        if (pattern.IsMatch(subFilePath))
                        {
                            //  デバッグ用
                            //Console.WriteLine("[file]" + subFilePath);
                            list.Add(subFilePath);
                        }
                    }
                }
                foreach (string subDirPath in Directory.GetDirectories(dirPath))
                {
                    if (isDirectory)
                    {
                        if (pattern.IsMatch(subDirPath))
                        {
                            //  デバッグ用
                            //Console.WriteLine("[directory]" + subDirPath);
                            list.Add(subDirPath);
                        }
                    }
                    if (dirPath != parentPath)
                    {
                        recurse(subDirPath);
                    }
                }
            };

            string wildParent = System.IO.Path.GetDirectoryName(path.Substring(0, path.IndexOf("*")));
            recurse(wildParent);

            return list.Count > 0;
        }

        /// <summary>
        /// ワイルドカードを含むレジストリキー/レジストリ名から有無チェック
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool SearchWildRegistry(string path, string name = null)
        {
            var list = new List<string>();
            var keyPatten = path.Contains("*") ? path.GetWildcardPattern() : null;
            var valPattern = (name?.Contains("*") ?? false) ? name.GetWildcardPattern() : null;

            Action<RegistryKey> checkRegName = (targetRegkey) =>
            {
                var valNames = name.Contains("*") ?
                    targetRegkey.GetValueNames().Where(x => valPattern.IsMatch(x)) :
                    targetRegkey.GetValueNames().Where(x => x.Equals(name, StringComparison.OrdinalIgnoreCase));
                foreach (string valName in valNames)
                {
                    string text = string.Format("Key={0} Name={1}", targetRegkey.ToString(), valName);
                    Console.WriteLine(text);
                    list.Add(text);
                }
            };

            if (path.Contains("*"))
            {
                string parentPath = System.IO.Path.GetDirectoryName(path.TrimEnd('\\'));

                Action<RegistryKey> recurse = null;
                recurse = (regKey) =>
                {
                    if (name != null)
                    {
                        checkRegName(regKey);
                    }

                    string regKeyStr = regKey.ToString();
                    foreach (var subKeyName in regKey.GetSubKeyNames())
                    {
                        using (var subRegKey = regKey.OpenSubKey(subKeyName))
                        {
                            if (name == null)
                            {
                                if (keyPatten.IsMatch(subRegKey.ToString()))
                                {
                                    string text = string.Format("Key={0}", subRegKey.ToString());
                                    Console.WriteLine(text);
                                    list.Add(text);
                                }
                            }

                            if (regKeyStr != parentPath)
                            {
                                recurse(subRegKey);
                            }
                        }
                    }
                };

                string wildParent = System.IO.Path.GetDirectoryName(path.Substring(0, path.IndexOf("*")));
                using (var parentRegKey = RegistryControl.GetRegistryKey(wildParent, false, false))
                {
                    recurse(parentRegKey);
                }
            }
            else
            {
                using (var regKey = RegistryControl.GetRegistryKey(path, false, false))
                {
                    if (regKey == null) { return false; }

                    if (name == null)
                    {
                        string text = string.Format("Key={0}", regKey.ToString());
                        Console.WriteLine(text);
                        list.Add(text);
                    }
                    else
                    {
                        checkRegName(regKey);
                    }
                }
            }

            return list.Count > 0;
        }

        #endregion
    }
}
