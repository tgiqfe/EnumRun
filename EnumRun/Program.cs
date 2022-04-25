
using EnumRun.Lib;
using EnumRun;
using System.IO;



EnumRunSetting setting = new EnumRunSetting();
setting.SetDefault();
setting.Serialize(@"Setting.json");

LanguageCollection collection = LanguageCollection.Deserialize();
collection.Save("Language.json");





/*
LanguageCollection collection = LanguageCollection.Deserialize();
EnumRunSetting setting = EnumRunSetting.Deserialize();
ProcessRange range = setting.Ranges[Item.AssemblyFile];
setting.Ranges.SetCurrentRange();

foreach (var scriptPath in Directory.GetFiles(setting.FilesPath))
{
    new Script(scriptPath, collection, range);
}
*/



Console.ReadLine();


