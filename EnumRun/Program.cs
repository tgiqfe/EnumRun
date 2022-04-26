
using EnumRun.Lib;
using EnumRun;
using System.IO;


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

if (Directory.Exists(setting.FilesPath))
{
    foreach (var scriptPath in Directory.GetFiles(setting.FilesPath))
    {
        var script = new Script(scriptPath, setting, collection);
    }
}



OptionType opt = OptionType.Output | OptionType.NoRun | OptionType.BeforeWait;
Console.WriteLine(opt.ToString());



Console.ReadLine();


