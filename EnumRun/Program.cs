
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

foreach (var scriptPath in Directory.GetFiles(setting.FilesPath))
{
    var script = new Script(scriptPath, setting, collection);
}



//Console.ReadLine();


