
using EnumRun.Lib;
using EnumRun;
using EnumRun.Log;
using System.IO;




EnumRunSetting setting_def = new EnumRunSetting();
setting_def.SetDefault();
setting_def.Serialize("Setting.json");
setting_def.Serialize("Setting.txt");

LanguageCollection collection_def = LanguageCollection.Deserialize();
collection_def.Save("Language.json");






LanguageCollection collection = LanguageCollection.Deserialize();
EnumRunSetting setting = EnumRunSetting.Deserialize();

using (var logger = new Logger(
    setting.LogsPath,
    "{0}_{1}.log", Item.ExecFileName, DateTime.Now.ToString("yyyyMMdd")))
{
    logger.Write("開始");

    ExecSessionResult check = ExecSession.Check(setting);
    logger.Write(check.GetMessage());
    if (check.Runnable)
    {
        if (!string.IsNullOrEmpty(setting.FilesPath) && Directory.Exists(setting.FilesPath))
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


