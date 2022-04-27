
using EnumRun.Lib;
using EnumRun;
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

ExecSessionResult check = ExecSession.Check(setting);
if (check.Runnable)
{
    Console.WriteLine("開始");
}

/*
if (Directory.Exists(setting.FilesPath))
{
    var processes = Directory.GetFiles(setting.FilesPath).
        ToList().
        Select(x => new Script(x, setting, collection)).
        Where(x => x.Enabled).
        Select(x => x.Process());
    Task.WhenAll(processes);
}
*/


Console.ReadLine();


