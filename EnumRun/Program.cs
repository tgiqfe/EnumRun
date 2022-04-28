
using EnumRun;
using EnumRun.Lib;
using EnumRun.Log;



bool initial = false;
if (initial)
{
    EnumRunSetting setting_def = new EnumRunSetting();
    setting_def.SetDefault();
    setting_def.Serialize("Setting.json");
    setting_def.Serialize("Setting.txt");

    LanguageCollection collection_def = LanguageCollection.Deserialize();
    collection_def.Save("Language.json");
}




LanguageCollection collection = LanguageCollection.Deserialize();
EnumRunSetting setting = EnumRunSetting.Deserialize();

using (var logger = new Logger(setting))
{
    logger.Write(setting.ToLog());

    var result = ExecSession.Check(setting);
    OldFiles.Clean(setting);

    if (result.Runnable)
    {
        logger.Write(LogLevel.Debug, result.ToLog());

        var processes = Directory.GetFiles(setting.FilesPath).
            Select(x => new Script(x, setting, collection, logger)).
            ToArray().
            Where(x => x.Enabled).
            Select(x => x.Process());

        Task.WhenAll(processes);
    }
    else
    {
        logger.Write(LogLevel.Warn, result.ToLog());
    }
}

Console.ReadLine();


