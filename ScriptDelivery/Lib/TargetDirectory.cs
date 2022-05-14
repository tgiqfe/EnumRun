using System.IO;
using System;
using ScriptDelivery;

namespace ScriptDelivery.Lib
{
    internal class TargetDirectory
    {
        /// <summary>
        /// 対象ファイルの親フォルダーを作成
        /// </summary>
        /// <param name="targetPath"></param>
        public static void CreateParent(string targetPath)
        {
            if (targetPath.Contains(Path.DirectorySeparatorChar))
            {
                string parent = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(parent))
                {
                    Directory.CreateDirectory(parent);
                }
            }
        }

        /// <summary>
        /// 対象ファイルの保存先フォルダーの候補
        /// </summary>

        private static readonly string[] _workCandidate = new string[]
        {
            //Item.WorkDirectoryPath,
            Item.ExecDirectoryPath,
        };


        /// <summary>
        /// 対象ファイルを取得
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFile(string fileName)
        {
            var filePath = _workCandidate.Select(x => Path.Combine(x, fileName)).
                FirstOrDefault(x => File.Exists(x));
            return filePath ?? Path.Combine(_workCandidate[0], fileName);
        }
    }
}
