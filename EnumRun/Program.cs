﻿
using EnumRun;
using EnumRun.Lib;
using EnumRun.Log;
using EnumRun.Log.ProcessLog;
using EnumRun.Log.MachineLog;

bool initial = false;
if (initial)
{
    EnumRunSetting setting_def = EnumRunSetting.Deserialize();
    setting_def.Serialize(Item.CONFIG_JSON);
    setting_def.Serialize(Item.CONFIG_TXT);

    LanguageCollection collection_def = LanguageCollection.Deserialize();
    collection_def.Save(Item.LANG_JSON);

    Console.WriteLine("設定ファイルを再セット");
    Console.ReadLine();
    Environment.Exit(0);
}


LanguageCollection collection = LanguageCollection.Deserialize();
EnumRunSetting setting = EnumRunSetting.Deserialize();

using (var logger = new ProcessLogger(setting))
{
    logger.Write(setting.ToLog());

    var session = new ExecSession(setting, logger);
    session.PreProcess();

    if(session.Enabled)
    {
        var processes = Directory.GetFiles(setting.GetFilesPath()).
            Select(x => new Script(x, setting, collection, logger)).
            ToArray().
            Where(x => x.Enabled).
            Select(x => x.Process());

        Task.WhenAll(processes);
    }

    session.PostProcess();
}

Console.ReadLine();


