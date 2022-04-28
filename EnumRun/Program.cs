
using EnumRun;
using EnumRun.Lib;
using EnumRun.Log;



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

    //ExecSessionResult check = ExecSession.Check(setting);
    var check = ExecSession2.Check(setting);

    if (check.Runnable)
    {
        logger.Write(LogLevel.Debug, check.ToLog());

        var processes = Directory.GetFiles(setting.FilesPath).
            Select(x => new Script(x, setting, collection, logger)).
            ToArray().
            Where(x => x.Enabled).
            Select(x => x.Process());
        //Task.WhenAll(processes);
    }
    else
    {
        logger.Write(LogLevel.Warn, check.ToLog());
    }
}

Console.ReadLine();


