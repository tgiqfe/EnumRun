using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;
using EnumRun.ScriptDelivery;

namespace EnumRun.Logs
{
    /// <summary>
    /// ScriptDeliveryサーバ宛に型未指定でログ転送
    /// </summary>
    internal class TransportDynamicLog
    {
        private ScriptDeliverySession _session = null;
        private JsonSerializerOptions _options = null;
        public bool Enabled { get; set; }

        public TransportDynamicLog(ScriptDeliverySession session)
        {
            this._session = session;
            this._options = new System.Text.Json.JsonSerializerOptions()
            {
                //Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                IgnoreReadOnlyProperties = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                //WriteIndented = true,
                //Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };
            this.Enabled = session.EnableLogTransport && session.Enabled;
        }

        public async Task<bool> SendAsync(string table, string json)
        {
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            using (var response = await _session.Client.PostAsync(_session.Uri + $"/logs/{table}", content))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                return false;
            }
        }

        public async Task<bool> SendAsync(string table, object obj)
        {
            return await SendAsync(table, JsonSerializer.Serialize(obj, _options));
        }

    }
}
