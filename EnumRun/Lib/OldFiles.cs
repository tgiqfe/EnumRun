﻿using System;
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
        //  (案)ExecSessionの中に組み込みに。Clean専用のログファイルはちょっともったいないかも

        public class CleanLog
        {
            public DateTime? LastCleanDate { get; set; }
            public int Retention { get; set; }
            public Dictionary<string, List<string>> DeletedFiles { get; set; }

            public CleanLog() { }
            public CleanLog(int retention)
            {
                this.LastCleanDate = DateTime.Now;
                this.Retention = retention;
                this.DeletedFiles = new Dictionary<string, List<string>>();
            }
        }

        public static void Clean(EnumRunSetting setting)
        {
            string filePath = TargetDirectory.GetFile(Item.CLEAN_FILE);
            try
            {
                using (var sr = new StreamReader(filePath, Encoding.UTF8))
                {
                    var lastCleanLog = JsonSerializer.Deserialize<CleanLog>(sr.ReadToEnd());
                    if (lastCleanLog.LastCleanDate?.Date >= DateTime.Today)
                    {
                        return;
                    }
                }
            }
            catch { }

            var cleanLog = new CleanLog(setting.RetentionPeriod ?? 0);
            DeleteOldFile(setting.OutputPath, cleanLog);
            DeleteOldFile(setting.LogsPath, cleanLog);

            using (var sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                string json = JsonSerializer.Serialize(cleanLog);
                sw.WriteLine(json);
            }
        }

        public static void DeleteOldFile(string targetDirectory, CleanLog cleanLog)
        {
            if (cleanLog.Retention > 0)
            {
                DateTime border = DateTime.Now.AddDays(cleanLog.Retention * -1);

                var files = (Directory.Exists(targetDirectory) ?
                    Directory.GetFiles(targetDirectory) :
                    new string[] { }).
                        Where(x => new FileInfo(x).LastWriteTime < border);
                try
                {
                    cleanLog.DeletedFiles[targetDirectory] = new List<string>();
                    foreach (var target in files)
                    {
                        File.Delete(target);
                        cleanLog.DeletedFiles[targetDirectory].Add(target);
                    }
                }
                catch { }
            }
        }
    }
}