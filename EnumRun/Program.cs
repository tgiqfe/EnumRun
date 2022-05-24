
using EnumRun;
using EnumRun.Lib;
using EnumRun.Logs;
using EnumRun.Logs.ProcessLog;
using EnumRun.ScriptDelivery;

bool initial = false;
if (initial)
{
    EnumRunSetting setting_def = EnumRunSetting.Deserialize();
    setting_def.Serialize(Item.CONFIG_JSON);
    setting_def.Serialize(Item.CONFIG_TXT);
    setting_def.Serialize(Item.CONFIG_YML);

    LanguageCollection collection_def = LanguageCollection.Deserialize();
    collection_def.Save(Item.LANG_JSON);

    Console.WriteLine("設定ファイルを再セット");
    Console.ReadLine();
    Environment.Exit(0);
}


LanguageCollection collection = LanguageCollection.Deserialize();
EnumRunSetting setting = EnumRunSetting.Deserialize();

using (var sdSession = new ScriptDeliverySession(setting))
using (var logger = new ProcessLogger(setting, sdSession))
{
    logger.Write(setting.ToLog());

    //  セッション開始時処理
    var exSession = new ExecSession(setting, logger);
    exSession.PreProcess();

    //  ScriptDeliveryサーバからスクリプトをダウンロード
    //var sdc = new ScriptDeliveryClient(setting, logger);
    var sdc = new ScriptDeliveryClient(sdSession, setting.FilesPath, setting.LogsPath, setting.ScriptDelivery?.TrashPath, logger);
    sdc.StartDownload();

    if (exSession.Enabled && Directory.Exists(setting.GetFilesPath()))
    {
        var processes = Directory.GetFiles(setting.GetFilesPath()).
            Select(x => new Script(x, setting, collection, logger)).
            ToArray().
            Where(x => x.Enabled).
            Select(x => x.Process());

        Task.WhenAll(processes);
    }

    //  セッション終了時処理
    exSession.PostProcess();
}

Console.ReadLine();


