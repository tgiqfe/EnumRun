
using EnumRun.Lib;
using EnumRun;


EnumRunSetting setting = new EnumRunSetting();
setting.SetDefault();
setting.Serialize(@"Setting.json");
setting.Languages.Save(@"Language.json");

/*
EnumRunSetting setting = EnumRunSetting.Deserialize();
setting.SetLanguageCollection();
*/





Console.ReadLine();




//Console.ReadLine();