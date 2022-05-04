using System.Management;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

namespace EnumRun.Log.ProcessLog
{
    internal class LogBody
    {
        public const string TAG = "ProcessLog";

        #region Serial parameter

        /// <summary>
        /// LiteDBに格納時にユニークキーとして使用
        /// </summary>
        [JsonIgnore]
        [LiteDB.BsonId]
        public string Serial { get; set; }

        /// <summary>
        /// Serialの通し番号
        /// </summary>
        private static int _index = 0;

        #endregion
        #region Public parameter

        public string Tag { get { return TAG; } }
        public string Date { get; set; }
        public LogLevel Level { get; set; }
        public string ProcessName { get; set; }
        public string ScriptFile { get; set; }
        public string UserName { get; set; }
        public string HostName { get; set; }
        public string Message { get; set; }

        #endregion
        
        private static JsonSerializerOptions _options = new JsonSerializerOptions()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            //IgnoreReadOnlyProperties = true,
            //DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            //WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        public LogBody() { }
        public LogBody(bool init)
        {
            this.ProcessName = Item.ProcessName;
            this.UserName = Environment.UserName;
            this.HostName = Environment.MachineName;
            this.Serial = $"{Item.Serial}_{_index++}";
        }

        public string GetJson()
        {
            return JsonSerializer.Serialize(this, _options);
        }
    }
}
