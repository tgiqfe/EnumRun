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
        private string _tableName = null;
        public bool Enabled { get; set; }

        public TransportDynamicLog(ScriptDeliverySession session, string tableName)
        {
            this._session = session;
            this._options = Item.GetJsonSerializerOption(
                escapeDoubleQuote: false,
                ignoreReadOnly: true,
                ignoreNull: true,
                writeIndented: false,
                convertEnumCamel: false);
            this._tableName = tableName;
            this.Enabled = session.EnableLogTransport && session.Enabled;
            _tableName = tableName;
        }

        public async Task<bool> SendAsync(string json)
        {
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            using (var response = await _session.Client.PostAsync(_session.Uri + $"/logs/{_tableName}", content))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                return false;
            }
        }
    }
}
