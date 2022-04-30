﻿using System.Management;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

namespace EnumRun.Log
{
    internal class LogBody
    {
        #region Serial parameter

        /// <summary>
        /// LiteDBに格納時にユニークキーとして使用
        /// </summary>
        [JsonIgnore]
        public string Serial
        {
            get
            {
                if (_seed == null)
                {
                    var md5 = System.Security.Cryptography.MD5.Create();
                    var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(
                        DateTime.Now.Ticks.ToString() + this.GetHashCode().ToString()));
                    _seed = BitConverter.ToString(bytes).Replace("-", "");
                    md5.Clear();
                }
                return $"{_seed}_{_index}";
            }
            set { _seed = value; }
        }
        private string _seed = null;
        private int _index = 0;

        #endregion

        public string Tag { get { return "EnumRun_Log"; } }
        public string Date { get; set; }
        public LogLevel Level { get; set; }
        public string ProcessName { get; set; }
        public string ScriptFile { get; set; }
        public string UserName { get; set; }
        public string HostName { get; set; }
        public string Message { get; set; }

        private JsonSerializerOptions _options = new JsonSerializerOptions()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            //WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        public void Init()
        {
            this.ProcessName = Item.ProcessName;
            this.UserName = Environment.UserName;
            this.HostName = Environment.MachineName;
        }

        public string GetLog(LogLevel level, string scriptFile, string message)
        {
            this.Date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            this.Level = level;
            this.ScriptFile = scriptFile;
            this.Message = message;
            this._index++;

            return JsonSerializer.Serialize(this, _options);
        }
    }
}
