
using EnumRun;
using EnumRun.Lib;
using EnumRun.Logs;
using EnumRun.Logs.ProcessLog;
using EnumRun.Logs.MachineLog;

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

    //  セッション開始時処理
    var session = new ExecSession(setting, logger);
    session.PreProcess();

    //  必要に応じてScriptDeliveryサーバからスクリプトをダウンロード
    var sdc = new ScriptDeliveryClient(setting, logger);
    sdc.StartDownload();

    if (session.Enabled)
    {
        var processes = Directory.GetFiles(setting.GetFilesPath()).
            Select(x => new Script(x, setting, collection, logger)).
            ToArray().
            Where(x => x.Enabled).
            Select(x => x.Process());

        Task.WhenAll(processes);
    }

    //  セッション終了時処理
    session.PostProcess();
}

Console.ReadLine();


