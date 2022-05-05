using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using EnumRun.Lib;

namespace EnumRun.Log
{
    internal class LogstashTransport
    {
        public bool Enabled { get; set; }

        private HttpRequestMessage _request = null;

        public LogstashTransport() { }
        public LogstashTransport(string logstashServer)
        {
            var info = new ServerInfo(logstashServer, 80, "http");

            //  接続可否チェック
            if (new TcpConnect(info.Server, info.Port).TcpConnectSuccess)
            {
                this.Enabled = true;

                //  Requestの基本情報部分を事前生成
                this._request = new HttpRequestMessage(HttpMethod.Post, logstashServer);
            }
        }

        /// <summary>
        /// JSON情報をLogstashサーバに転送
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public async Task<bool> SendAsync(string json)
        {
            if(_request == null)
            {
                return false;
            }
            using (var client = new HttpClient())
            {
                _request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(_request);

                return response.StatusCode == HttpStatusCode.OK;
            }
        }
    }
}
