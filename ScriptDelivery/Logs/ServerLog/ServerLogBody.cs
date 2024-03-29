﻿using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScriptDelivery.Logs.ServerLog
{
    internal class ServerLogBody : LogBodyBase
    {
        public const string TAG = "ScriptDelivery";

        #region Public parameter

        public override string Tag { get { return TAG; } }
        public override string Date { get; set; }
        public override string ProcessName { get; set; }
        public override string HostName { get; set; }
        public override string UserName { get; set; }
        public LogLevel Level { get; set; }
        public string Client { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }

        #endregion

        private static int _index = 0;
        private static JsonSerializerOptions _options = null;

        public ServerLogBody() { }
        public ServerLogBody(bool init)
        {
            if (init)
            {
                ProcessName = Item.ProcessName;
                HostName = Environment.MachineName;
                UserName = Environment.UserName;
                Serial = $"{Item.Serial}_{_index++}";
            }
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

        public override string ToConsoleMessage()
        {
            return string.Format("[{0}][{1}] Client:{2} Title:{3} Message:{4}",
                this.Date,
                this.Level,
                this.Client ?? "-",
                this.Title ?? "-",
                this.Message);
        }
    }
}
