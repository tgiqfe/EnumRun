using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScriptDelivery.Logs
{
    internal class LogBodyBase
    {
        /// <summary>
        /// LiteDBに格納時にユニークキーとして使用
        /// </summary>
        [JsonIgnore]
        [LiteDB.BsonId]
        public string Serial { get; set; }

        #region Public parameter

        public virtual string Tag { get { return ""; } }
        public virtual string Date { get; set; }
        public virtual string ProcessName { get; set; }
        public virtual string HostName { get; set; }
        public virtual string UserName { get; set; }

        #endregion

        protected JsonSerializerOptions GetJsonSerializerOption(
            bool escapeDoubleQuote,
            bool ignoreReadOnly,
            bool ignoreNull,
            bool writeIndented,
            bool convertEnumCamel)
        {
            var options = convertEnumCamel ?
                new JsonSerializerOptions() { Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) } } :
                new JsonSerializerOptions();
            if (escapeDoubleQuote)
            {
                options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            }
            if (ignoreReadOnly)
            {
                options.IgnoreReadOnlyProperties = true;
            }
            if (ignoreNull)
            {
                options.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            }
            if (writeIndented)
            {
                options.WriteIndented = true;
            }

            return options;
        }

        public virtual string GetJson() { return ""; }

        public virtual Dictionary<string, string> SplitForSyslog() { return null; }

        public virtual string ToConsoleMessage() { return ""; }
    }
}
