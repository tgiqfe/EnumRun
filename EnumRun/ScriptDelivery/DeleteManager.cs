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

        private string _targetDir = null;
        private string _trashPath = null;
        private EnumRun.Logs.ProcessLog.ProcessLogger _logger = null;
        private List<string> _targets { get; set; }
        private List<string> _excludes { get; set; }

        private List<string> _fList = null;
        private List<string> _dList = null;

        public DeleteManager(string targetDir, string trashPath, EnumRun.Logs.ProcessLog.ProcessLogger logger)
        {
            this._targetDir = Path.GetFullPath(targetDir);
            this._trashPath = trashPath;
            this._logger = logger;
            this._targets = new List<string>();
            this._excludes = new List<string>();
        }

        public void Process()
        {
            SearchTarget();
            DeleteTarget();
        }

        public void AddTarget(string[] array)
        {
            if(array?.Length > 0)
            {
                this._targets.AddRange(array);
            }
        }

        public void AddExclude(string[] array)
        {
            if(array?.Length > 0)
            {
                this._excludes.AddRange(array);
            }
        }

        private void SearchTarget()
        {
            if (!Directory.Exists(_targetDir)) { return; }

            List<SearchPath> targetList = new List<SearchPath>(_targets.Select(x => new SearchPath(_targetDir, x)));
            List<SearchPath> excludeList = new List<SearchPath>(_excludes.Select(x => new SearchPath(_targetDir, x)));

            _fList = Directory.GetFiles(_targetDir, "*", SearchOption.AllDirectories).
                Where(x => targetList.Any(y => y.IsMatch(x))).
                ToList();
            _dList = Directory.GetDirectories(_targetDir, "*", SearchOption.AllDirectories).
                Where(x => targetList.Any(y => y.IsMatch(x))).
                ToList();

            _logger.Write(Logs.LogLevel.Debug, null, "Delete [file => {0}, directory => {1}]", _fList.Count, _dList.Count);

            for (int i = _dList.Count - 1; i >= 0; i--)
            {
                var matchSearch = excludeList.FirstOrDefault(x => x.IsMatch(_dList[i]));
                if (matchSearch != null)
                {
                    _logger.Write(Logs.LogLevel.Info, null, "Exclude directory, [{0}]", _dList[i]);
                    
                    string dirName = _dList[i];
                    _dList.RemoveAt(i);

                    _fList.Where(x => x.StartsWith(dirName + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)).
                        ToList().
                        ForEach(x =>
                        {
                            _logger.Write(Logs.LogLevel.Info, null, "Exclude lower path file, [{0}]", x);
                        });
                    _dList.Where(x => x.StartsWith(dirName + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)).
                        ToList().
                        ForEach(x =>
                        {
                            _logger.Write(Logs.LogLevel.Info, null, "Exclude lower path directory, [{0}]", x);
                        });
                    _fList.RemoveAll(x => x.StartsWith(dirName + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));
                    _dList.RemoveAll(x => x.StartsWith(dirName + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));
                }
            }
            for (int i = _fList.Count - 1; i >= 0; i--)
            {
                var matchSearch = excludeList.FirstOrDefault(x => x.IsMatch(_fList[i]));
                if (matchSearch != null)
                {
                    _logger.Write(Logs.LogLevel.Info, null, "Exclude file, [{0}]", _fList[i]);

                    _dList.Where(x => _fList[i].StartsWith(x + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)).
                        ToList().
                        ForEach(x =>
                        {
                            _logger.Write(Logs.LogLevel.Info, null, "Exclude upper path directory, [{0}]", x);
                        });

                    _dList.RemoveAll(x => _fList[i].StartsWith(x + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));
                    _fList.RemoveAt(i);
                }
            }

            _logger.Write(Logs.LogLevel.Debug, null, "Delete [file => {0}, directory => {1}]", _fList.Count, _dList.Count);
        }

        private void DeleteTarget()
        {
            foreach (string delTarget in _dList ?? new List<string>())
            {
                try
                {
                    if (string.IsNullOrEmpty(_trashPath))
                    {
                        Directory.Delete(delTarget, true);

                        _logger.Write(Logs.LogLevel.Info, null, "Delete directory, [{0}]", delTarget);
                    }
                    else
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

                        _logger.Write(Logs.LogLevel.Info, null, "ToTrash directory, [{0} -> {1}]", delTarget, destination);
                    }
                }
                catch { }
            }
            foreach (string delTarget in _fList ?? new List<string>())
            {
                if (File.Exists(delTarget))
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_trashPath))
                        {
                            File.Delete(delTarget);
                            _logger.Write(Logs.LogLevel.Info, null, "Delete file, [{0}]", delTarget);
                        }
                        else
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

                            _logger.Write(Logs.LogLevel.Info, null, "ToTrash file, [{0} -> {1}]", delTarget, destination);
                        }
                    }
                    catch { }
                }
            }
        }
    }
}
