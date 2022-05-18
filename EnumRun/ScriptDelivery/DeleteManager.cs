using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using EnumRun.Lib;

namespace EnumRun.ScriptDelivery
{
    /// <summary>
    /// 対象ファイル/フォルダーの削除対象と削除対象外を管理
    /// </summary>
    internal class DeleteManager
    {
        #region Inner class

        public class SearchPath
        {
            public string FullPath { get; set; }
            public string AsDirectoryPath { get; set; }
            public System.Text.RegularExpressions.Regex Pattern { get; set; }

            public SearchPath(string baseDir, string path)
            {
                this.FullPath = Path.GetFullPath(Path.Combine(baseDir, path.TrimEnd('\\')));
                this.AsDirectoryPath = FullPath + Path.DirectorySeparatorChar;
                if (FullPath.Contains("*"))
                {
                    this.Pattern = FullPath.GetWildcardPattern();
                }
            }
            public bool IsMatch(string target)
            {
                if (Pattern == null)
                {
                    return this.FullPath.Equals(target, StringComparison.OrdinalIgnoreCase) ||
                        target.StartsWith(this.AsDirectoryPath);
                }
                return this.Pattern.IsMatch(target);
            }
        }

        #endregion

        public List<string> Targetlist { get; set; }
        public List<string> ExcludeList { get; set; }

        private List<string> _fList = null;
        private List<string> _dList = null;
        private string _targetDir = null;
        private string _trashPath = null;

        public DeleteManager(string targetDir, string trashPath)
        {
            this._targetDir = Path.GetFullPath(targetDir);
            this._trashPath = trashPath;
            this.Targetlist = new List<string>();
            this.ExcludeList = new List<string>();
        }

        public void Process()
        {
            SearchTarget();
            DeleteTarget();
        }

        private void SearchTarget()
        {
            List<SearchPath> _targetList = new List<SearchPath>(Targetlist.Select(x => new SearchPath(_targetDir, x)));
            List<SearchPath> _excludeList = new List<SearchPath>(ExcludeList.Select(x => new SearchPath(_targetDir, x)));

            _fList = Directory.GetFiles(_targetDir, "*", SearchOption.AllDirectories).
                Where(x => _targetList.Any(y => y.IsMatch(x))).
                ToList();
            _dList = Directory.GetDirectories(_targetDir, "*", SearchOption.AllDirectories).
                Where(x => _targetList.Any(y => y.IsMatch(x))).
                ToList();

            Console.WriteLine("fList => {0}, dList => {1}", _fList.Count, _dList.Count);

            for (int i = _dList.Count - 1; i >= 0; i--)
            {
                var matchSearch = _excludeList.FirstOrDefault(x => x.IsMatch(_dList[i]));
                if (matchSearch != null)
                {
                    Console.WriteLine("■;[ExcD] {0}", _dList[i]);

                    _fList.Where(x => x.StartsWith(_dList[i] + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)).
                        ToList().
                        ForEach(x =>
                        {
                            Console.WriteLine("■:[SubF] {0}", x);
                        });
                    _fList.RemoveAll(x => x.StartsWith(_dList[i] + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));
                    _dList.RemoveAt(i);
                }
            }
            for (int i = _fList.Count - 1; i >= 0; i--)
            {
                var matchSearch = _excludeList.FirstOrDefault(x => x.IsMatch(_fList[i]));
                if (matchSearch != null)
                {
                    Console.WriteLine("★;[ExcF] {0}", _fList[i]);

                    _dList.Where(x => _fList[i].StartsWith(x + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)).
                        ToList().
                        ForEach(x =>
                        {
                            Console.WriteLine("★:[SubD] {0}", x);
                        });

                    _dList.RemoveAll(x => _fList[i].StartsWith(x + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));
                    _fList.RemoveAt(i);
                }
            }

            Console.WriteLine("fList => {0}, dList => {1}", _fList.Count, _dList.Count);
        }

        private void DeleteTarget()
        {
            foreach (string delTarget in _dList)
            {
                try
                {
                    string destination = Path.Combine(_trashPath, Path.GetRelativePath(_targetDir, delTarget));
                    string parent = Path.GetDirectoryName(destination);
                    if (!Directory.Exists(parent))
                    {
                        Directory.CreateDirectory(parent);
                    }
                    if (Directory.Exists(destination))
                    {
                        Directory.Delete(destination, true);
                    }
                    Directory.Move(delTarget, destination);

                    Console.WriteLine("▲;[Del] {0}", destination);
                }
                catch { }
            }
            foreach (string delTarget in _fList)
            {
                if (File.Exists(delTarget))
                {
                    try
                    {
                        string destination = Path.Combine(_trashPath, Path.GetRelativePath(_targetDir, delTarget));
                        string parent = Path.GetDirectoryName(destination);
                        if (!Directory.Exists(parent))
                        {
                            Directory.CreateDirectory(parent);
                        }
                        if (File.Exists(destination))
                        {
                            File.Delete(destination);
                        }
                        File.Move(delTarget, destination);

                        Console.WriteLine("▲:[Del] {0}", destination);
                    }
                    catch { }
                }
            }
        }
    }
}
