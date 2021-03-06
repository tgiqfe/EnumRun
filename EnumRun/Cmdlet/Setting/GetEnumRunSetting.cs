using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using EnumRun.Serialize;
using System.IO;

namespace EnumRun.Cmdlet
{
    [Cmdlet(VerbsCommon.Get, "EnumRunSetting")]
    public class GetEnumRunSetting : PSCmdlet
    {
        [Parameter(Position = 0), Alias("Path")]
        public string SettingPath { get; set; }
        [Parameter]
        public SwitchParameter Version { get; set; }

        private EnumRunSetting _setting = null;

        protected override void BeginProcessing()
        {
            if (string.IsNullOrEmpty(SettingPath))
            {
                SettingPath = File.Exists(Item.CURRENT_DIR_SETTING) ? Item.CURRENT_DIR_SETTING : Item.PROGRAMDATA_SETTING;
            }
            _setting = DataSerializer.Deserialize<EnumRunSetting>(SettingPath);
        }

        protected override void ProcessRecord()
        {
            if (Version)
            {
                WriteObject("EnumRun - v" + Function.GetVersion());
            }
            else
            {
                WriteObject(_setting, true);
            }
        }
    }
}
