
using EnumRun.Lib;
using EnumRun;
using EnumRun.Log;
using System.IO;



/*
EnumRunSetting setting_def = new EnumRunSetting();
setting_def.SetDefault();
setting_def.Serialize("Setting.json");
setting_def.Serialize("Setting.txt");

LanguageCollection collection_def = LanguageCollection.Deserialize();
collection_def.Save("Language.json");
*/

LanguageCollection collection = LanguageCollection.Deserialize();
EnumRunSetting setting = EnumRunSetting.Deserialize();

using (var logger = new Logger(setting))
{
    logger.Write(setting.ToLog());

    ExecSessionResult check = ExecSession.Check(setting);
    if (check.Runnable)
    {
        logger.Write(LogLevel.Info, check.GetMessage());

        var processes = Directory.GetFiles(setting.FilesPath).
            Select(x => new Script(x, setting, collection, logger)).
            ToArray().
            Where(x => x.Enabled).
            Select(x => x.Process());
        //Task.WhenAll(processes);
    }
    else
    {
        logger.Write(LogLevel.Warn, check.GetMessage());
    }
}

Console.ReadLine();


