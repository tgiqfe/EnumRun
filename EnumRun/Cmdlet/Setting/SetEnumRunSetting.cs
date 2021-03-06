using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;
using EnumRun.Serialize;
using EnumRun.ScriptLanguage;

namespace EnumRun.Cmdlet
{
    [Cmdlet(VerbsCommon.Set, "EnumRunSetting")]
    public class SetEnumRunSetting : PSCmdlet
    {
        [Parameter(Position = 0), Alias("Path")]
        public string SettingPath { get; set; }
        [Parameter]
        public string FilesPath { get; set; }
        [Parameter]
        public string LogsPath { get; set; }
        [Parameter]
        public string OutputPath { get; set; }
        [Parameter]
        public bool? DebugMode { get; set; }
        [Parameter]
        public bool? RunOnce { get; set; }
        [Parameter]
        public Range[] Ranges { get; set; }
        [Parameter]
        public Language[] Languages { get; set; }
        [Parameter]
        public SwitchParameter Clear { get; set; }

        private EnumRunSetting _setting = null;

        protected override void BeginProcessing()
        {
            //  設定ファイルの場所を探す優先度
            //  1. アセンブリと同じ場所の setting.json
            //  2. C:\ProgramData\EnumRun の setting.json
            if (string.IsNullOrEmpty(SettingPath))
            {
                SettingPath = File.Exists(Item.CURRENT_DIR_SETTING) ? Item.CURRENT_DIR_SETTING : Item.PROGRAMDATA_SETTING;
            }
            _setting = DataSerializer.Deserialize<EnumRunSetting>(SettingPath);
        }

        protected override void ProcessRecord()
        {
            //  ClearもしくはEnumRunSettingがnullの場合、デフォルト設定で再作成
            if (_setting == null || Clear)
            {
                _setting = new EnumRunSetting();
                _setting.SetDefaultParameter();
            }

            //  スクリプトファイルの保存先
            if (!string.IsNullOrEmpty(FilesPath)) { _setting.FilesPath = FilesPath; }

            //  ログ出力先
            if (!string.IsNullOrEmpty(LogsPath)) { _setting.LogsPath = LogsPath; }

            //  コンソール出力内容のリダイレクト先
            if (!string.IsNullOrEmpty(OutputPath)) { _setting.OutputPath = OutputPath; }

            //  デバッグモード
            if (DebugMode != null) { _setting.DebugMode = (bool)DebugMode; }

            //  1回だけ実行
            if (RunOnce != null) { _setting.RunOnce = (bool)RunOnce; }

            //  Range設定
            if (Ranges != null && Ranges.Length > 0) { _setting.Ranges = new List<Range>(Ranges); }

            //  Languages設定
            if (Languages != null && Languages.Length > 0) { _setting.Languages = new List<Language>(Languages); }

            DataSerializer.Serialize<EnumRunSetting>(_setting, SettingPath);

            WriteObject(_setting, true);
        }
    }
}
