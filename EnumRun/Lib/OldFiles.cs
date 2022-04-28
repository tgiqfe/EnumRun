using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnumRun.Lib
{
    internal class OldFiles
    {
        public class CleanLog
        {
            public DateTime? LastCleanDate { get; set; }
        }

        public static void Clean(EnumRunSetting setting)
        {
            string filePath = new string[]
            {
                Path.Combine(Item.WorkDirectory, Item.CLEAN_FILE),
                Path.Combine(Item.ExecDirectoryPath, Item.CLEAN_FILE),
            }.FirstOrDefault(x => File.Exists(x));
            filePath ??= Path.Combine(Item.WorkDirectory, Item.CLEAN_FILE);

            try
            {
                using (var sr = new StreamReader(filePath, Encoding.UTF8))
                {
                    var cleanLog = JsonSerializer.Deserialize<CleanLog>(sr.ReadToEnd());
                    if (cleanLog.LastCleanDate?.Date >= DateTime.Today)
                    {
                        return;
                    }
                }
            }
            catch { }

            Cle(setting.OutputPath, setting.RetentionPeriod ?? 0);
            Cle(setting.LogsPath, setting.RetentionPeriod ?? 0);

            using(var sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                string json = JsonSerializer.Serialize(
                    new CleanLog() { LastCleanDate = DateTime.Now });
                sw.WriteLine(json);
            }

        }

        private static void Cle(string targetDirectory, int retention)
        {
            if(retention > 0)
            {
                DateTime borderDate = DateTime.Now.AddDays(retention * -1);

                var files = (Directory.Exists(targetDirectory) ?
                    Directory.GetFiles(targetDirectory) :
                    new string[] { }).
                        Where(x => new FileInfo(x).LastWriteTime < borderDate);
                foreach (var target in files)
                {
                    File.Delete(target);
                }
            }
        }

        //  (案)戻り値で削除したファイルを返す。
        //  (案)削除したファイル名をログもしくはCleanログに出力
    }
}
