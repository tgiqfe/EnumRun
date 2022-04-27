
using EnumRun.Lib;
using EnumRun;
using EnumRun.Log;
using System.IO;


/*
using (System.Diagnostics.Process proc = new System.Diagnostics.Process())
{
    Console.WriteLine(proc.Id);

    proc.StartInfo.FileName = "cmd";
    proc.StartInfo.Arguments = "/c ping localhost -n 5";
    proc.Start();
    proc.WaitForExit();
}
*/


/*
EnumRunSetting setting = new EnumRunSetting();
setting.SetDefault();
setting.Serialize("Setting.json");
setting.Serialize("Setting.txt");

LanguageCollection collection = LanguageCollection.Deserialize();
collection.Save("Language.json");
*/





LanguageCollection collection = LanguageCollection.Deserialize();
EnumRunSetting setting = EnumRunSetting.Deserialize();

using (var logger = new Logger(
    setting.LogsPath,
    "{0}_{1}.log", Item.ExecFileName, DateTime.Now.ToString("yyyyMMdd")))
{
    logger.Write("開始");

    ExecSessionResult check = ExecSession.Check(setting);
    if (check.Runnable)
    {
        
        if (Directory.Exists(setting.FilesPath))
        {
            var processes = Directory.GetFiles(setting.FilesPath).
                ToList().
                Select(x => new Script(x, setting, collection, logger)).
                Where(x => x.Enabled).
                Select(x => x.Process());
            //Task.WhenAll(processes);
        }
    }

    logger.Write("終了");
}

Console.ReadLine();


