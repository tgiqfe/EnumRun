using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace ScriptDelivery.Lib
{
    /// <summary>
    /// TCP接続可否チェック用
    /// </summary>
    internal class TcpConnect
    {
        private bool _reachable { get; set; }
        private bool _success { get; set; }

        public bool PingSuccess { get { return _reachable; } }
        public bool TcpConnectSuccess { get { return _success; } }

        public TcpConnect(string server, bool startTest = true)
        {
            if (startTest)
            {
                this.TestAsync(server).Wait();
            }
        }

        public TcpConnect(string server, int port, bool startTest = true)
        {
            if (startTest)
            {
                this.TestAsync(server, port).Wait();
            }
        }

        public bool Test(string server)
        {
            this.TestAsync(server).Wait();
            return _reachable;
        }

        public async Task<bool> TestAsync(string server)
        {
            int maxCount = 4;       //  最大4回チェック
            int interval = 500;     //  インターバル500ミリ秒
            Ping ping = new Ping();
            for (int i = 0; i < maxCount; i++)
            {
                PingReply reply = await ping.SendPingAsync(server);
                if (reply.Status == IPStatus.Success)
                {
                    this._reachable = true;
                    return true;
                }
                await Task.Delay(interval);
            }
            this._reachable = false;
            return false;
        }

        public bool Test(string server, int port)
        {
            this.TestAsync(server, port).Wait();
            return this._success;
        }

        public async Task<bool> TestAsync(string server, int port)
        {
            using (var client = new TcpClient())
            {
                int timeout = 3000;
                try
                {
                    Task task = (client.ConnectAsync(server, port));
                    if (await Task.WhenAny(task, Task.Delay(timeout)) != task)
                    {
                        throw new SocketException(10060);
                    }
                }
                catch { }

                this._success = client.Connected;
                return client.Connected;
            }
        }
    }
}
