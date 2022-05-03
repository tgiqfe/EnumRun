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

        private string _logstashServer { get; set; }
        private string _server = null;
        private int _port = 8080;
        private HttpRequestMessage _request = null;

        public LogstashTransport() { }
        public LogstashTransport(string logstashServer)
        {
            this._logstashServer = logstashServer;

            //  Logstashサーバ情報を格納
            string tempServer = logstashServer;
            string tempPort = "";
            if (tempServer.StartsWith("http://") || tempServer.StartsWith("https://"))
            {
                tempServer = tempServer.Substring(tempServer.IndexOf("//") + 2);
            }
            if (tempServer.Contains(":"))
            {
                tempPort = tempServer.Substring(tempServer.IndexOf(":") + 1);
                if (tempPort.Contains("/"))
                {
                    tempPort = tempPort.Substring(0, tempPort.IndexOf("/"));
                }
                tempServer = tempServer.Substring(0, tempServer.IndexOf(":"));
            }
            this._server = tempServer;
            this._port = int.Parse(tempPort);

            //  接続可否チェック
            if (new TcpConnect(_server, _port).TcpConnectSuccess)
            {
                this.Enabled = true;

                //  Requestの基本情報部分を事前生成
                this._request = new HttpRequestMessage(HttpMethod.Post, _logstashServer);
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


        /*
        /// <summary>
        /// LogstashサーバへTCP接続可否チェック
        /// </summary>
        /// <returns></returns>
        private async Task TestAsync()
        {
            using (var client = new TcpClient())
            {
                int timeout = 3000;
                try
                {
                    Task task = (client.ConnectAsync(_server, _port));
                    if (await Task.WhenAny(task, Task.Delay(timeout)) != task)
                    {
                        throw new SocketException(10060);
                    }
                }
                catch { }
                this.Enabled = client.Connected;
            }
        }
        */
    }
}
