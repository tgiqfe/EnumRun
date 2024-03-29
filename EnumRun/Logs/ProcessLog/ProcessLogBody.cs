﻿using System.Management;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

namespace EnumRun.Logs.ProcessLog
{
    internal class ProcessLogBody : LogBodyBase
    {
        public const string TAG = "ProcessLog";

        #region Public parameter

        public override string Tag { get { return TAG; } }
        public override string Date { get; set; }
        public override string ProcessName { get; set; }
        public override string HostName { get; set; }
        public override string UserName { get; set; }
        public override LogLevel Level { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }

        #endregion

        private static int _index = 0;
        private static JsonSerializerOptions _options = null;

        public ProcessLogBody() { }
        public ProcessLogBody(bool init)
        {
            this.ProcessName = Item.ProcessName;
            this.HostName = Environment.MachineName;
            this.UserName = Environment.UserName;
            this.Serial = $"{Item.Serial}_{_index++}";
        }

        public override string GetJson()
        {
            _options ??= Item.GetJsonSerializerOption(
                escapeDoubleQuote: true,
                false,
                false,
                false,
                convertEnumCamel: true);
            return JsonSerializer.Serialize(this, _options);
        }

        public override Dictionary<string, string> SplitForSyslog()
        {
            return new Dictionary<string, string>()
            {
                { Title, Message }
            };
        }
    }
}
