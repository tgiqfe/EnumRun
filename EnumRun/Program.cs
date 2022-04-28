﻿
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



/*
string[] aaaa = new string[] { "aa", "bb", "cc", "dd" };
//var keka = aaaa.FirstOrDefault(x => x == "aa");
var keka = aaaa.FirstOrDefault();

Console.WriteLine(keka);

Console.ReadLine();
Environment.Exit(0);
*/

using (var logger = new Logger(setting))
{
    logger.Write(setting.ToLog());

    var result = ExecSession.Check(setting);

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


