using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnumRun.Log.ResultLog
{
    //  (案)Resultは不要

    internal class ResultLogBody : LogBodyBase
    {
        public const string TAG = "ResultLog";

        #region Public parameter

        public override string Tag { get { return TAG; } }
        public override string Date { get; set; }
        public override string ProcessName { get; set; }
        public override string HostName { get; set; }
        public override string UserName { get; set; }

        public bool? OldFileClear { get; set; }         //  保持期間終了後のファイル掃除を実行したかどうか
        public List<string> ClearFiles { get; set; }    //  保持期間終了後に削除したファイルのリスト
        public bool? IsSendMachineLog { get; set; }     //  MachineLogを送信したかどうか
        public List<string> RunnedScripts { get; set; } //  実行したスクリプトファイル
        public List<string> NotRunnedScripts { get; set; }  //  実行しなかったスクリプトファイル

        #endregion

        private static int _index = 0;
        private static JsonSerializerOptions _options = null;

        public ResultLogBody() { }
        public ResultLogBody(bool init)
        {
            this.ProcessName = Item.ProcessName;
            this.HostName = Environment.MachineName;
            this.UserName = Environment.UserName;
            this.Serial = $"{Item.Serial}_{_index++}";




        }

        public override string GetJson()
        {
            _options ??= GetJsonSerializerOption(
                escapeDoubleQuote: true,
                false,
                false,
                false,
                convertEnumCamel: true);
            return JsonSerializer.Serialize(this, _options);
        }
    }
}
