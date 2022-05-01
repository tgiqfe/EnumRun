using System.Management;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

namespace EnumRun.Log.ProcessLog
{
    internal class LogBody
    {
        public const string TAG = "EnumRun_ProcessLog";

        #region Serial parameter

        /// <summary>
        /// LiteDBに格納時にユニークキーとして使用
        /// </summary>
        [JsonIgnore]
        [LiteDB.BsonId]
        public string Serial { get; set; }

        /// <summary>
        /// シリアル番号の元データ
        /// </summary>
        private string _seed = null;

        /// <summary>
        /// シリアル番号の末尾に付ける数値
        /// </summary>
        private int _index = 0;

        #endregion
        #region Public parameter

        public string Tag { get { return LogBody.TAG; } }
        public string Date { get; set; }
        public LogLevel Level { get; set; }
        public string ProcessName { get; set; }
        public string ScriptFile { get; set; }
        public string UserName { get; set; }
        public string HostName { get; set; }
        public string Message { get; set; }

        #endregion

        private JsonSerializerOptions _options = new JsonSerializerOptions()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            //IgnoreReadOnlyProperties = true,
            //DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            //WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        public void Init()
        {
            this.ProcessName = Item.ProcessName;
            this.UserName = Environment.UserName;
            this.HostName = Environment.MachineName;
        }

        public void Update(LogLevel level, string scriptFile, string message)
        {
            this.Date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            this.Level = level;
            this.ScriptFile = scriptFile;
            this.Message = message;

            if (_seed == null)
            {
                var md5 = System.Security.Cryptography.MD5.Create();
                var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(
                    DateTime.Now.Ticks.ToString() + this.GetHashCode().ToString()));
                _seed = BitConverter.ToString(bytes).Replace("-", "");
                md5.Clear();
            }
            this.Serial = $"{_seed}_{_index++}";
        }

        public string GetJson()
        {
            return JsonSerializer.Serialize(this, _options);
        }
    }
}
