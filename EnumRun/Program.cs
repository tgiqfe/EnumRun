using EnumRun;
using EnumRun.Lib;
using EnumRun.Logs.ProcessLog;
using EnumRun.ScriptDelivery;

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

using (var session = new ScriptDeliverySession(setting))
using (var logger = new ProcessLogger2(setting, session))
{
    logger.Write(setting.ToLog());

    //  セッション開始時処理
    var worker = new SessionWorker(setting, session, logger);
    worker.PreProcess();

    //  ScriptDeliveryサーバからスクリプトをダウンロード
    //var sdc = new ScriptDeliveryClient(session, setting.FilesPath, setting.LogsPath, setting.ScriptDelivery?.TrashPath, logger);
    var sdc = new ScriptDeliveryClient(setting, session, logger);
    sdc.StartDownload();

    if (worker.Enabled && Directory.Exists(setting.GetFilesPath()))
    {
        var processes = Directory.GetFiles(setting.GetFilesPath()).
            Select(x => new Script(x, setting, collection, logger)).
            ToArray().
            Where(x => x.Enabled).
            Select(x => x.Process());

        Task.WhenAll(processes);
    }

    //  セッション終了時処理
    worker.PostProcess();
}

Console.ReadLine();


