using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumRun.Lib;
using EnumRun.Lib.Infos;

namespace EnumRun.ScriptDelivery
{
    internal class ScriptDeliverySession : IDisposable
    {
        public bool Enabled { get; private set; }
        public string Uri { get; private set; }
        public HttpClient Client { get; private set; }

        public bool EnableDelivery { get; private set; }
        public bool EnableLogTransport { get; private set; }

        public ScriptDeliverySession(EnumRunSetting setting)
        {
            //  設定ファイルの記述で、以下の条件を満たす場合に実行
            //  - ScriptDeliveryの記述がある
            //  - ScriptDelivery.Serverの記述がある
            //  - 以下のどちらかが一致
            //    - SctiptDelivery.ProcessNameが一致 ←この場合は最大30秒待機
            //    - ScriptDelivery.LogTransportがtrue
            if (setting.ScriptDelivery != null && setting.ScriptDelivery.Server?.Length > 0)
            {
                this.EnableDelivery = setting.ScriptDelivery.Process?.Equals(Item.ProcessName, StringComparison.OrdinalIgnoreCase) ?? false;
                this.EnableLogTransport = setting.ScriptDelivery?.LogTransport ?? false;
                if (EnableDelivery || EnableLogTransport)
                {
                    var random = new Random();
                    string[] array = setting.ScriptDelivery.Server.OrderBy(x => random.Next()).ToArray();
                    foreach (var sv in array)
                    {
                        var info = new ServerInfo(sv, 5000, "http");
                        var connect = EnableDelivery ?
                            new TcpConnect(info.Server, info.Port, maxPingCount: 30) :
                            new TcpConnect(info.Server, info.Port);
                        if (connect.TcpConnectSuccess)
                        {
                            this.Uri = info.URI;
                            this.Client = new HttpClient();
                            this.Enabled = true;
                            break;
                        }
                    }
                }
            }
        }

        public void Close()
        {
            if (Client != null)
            {
                Client.Dispose();
            }
        }

        #region Dispose

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Close();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

