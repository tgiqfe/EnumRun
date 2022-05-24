using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnumRun.ScriptDelivery;

namespace EnumRun.Logs
{
    internal class DynamicLogTransport
    {
        private ScriptDeliverySession _session = null;
        private JsonSerializerOptions _options = null;
        public bool Enabled { get; set; }

        public DynamicLogTransport(ScriptDeliverySession session)
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

        public async Task<bool> SendAsync(string json)
        {
            //_logger.Write(LogLevel.Debug, "Search, download file from ScriptDelivery server.");

            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            using (var response = await _session.Client.PostAsync(_session.Uri + "/download/list", content))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    //_logger.Write(LogLevel.Info, "Success, download DownloadFile list object");
                    return true;
                }
                //_logger.Write(LogLevel.Error, "Failed, download DownloadFile list object");
                return false;
            }
        }
    }
}
