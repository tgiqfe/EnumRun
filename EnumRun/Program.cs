
using EnumRun.Lib;
using EnumRun;
using System.IO;


/*
EnumRunSetting setting = new EnumRunSetting();
setting.SetDefault();
setting.Serialize(@"Setting.json");
setting.Languages.Save(@"Language.json");
*/

LanguageCollection collection = LanguageCollection.Deserialize();
EnumRunSetting setting = EnumRunSetting.Deserialize();
ProcessRange range = setting.Ranges[Item.AssemblyFile];

foreach (var scriptPath in Directory.GetFiles(setting.FilesPath))
{
    new Script(scriptPath, collection, range);
}



Console.ReadLine();


