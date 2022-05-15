using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ScriptDelivery.Lib
{
    /// <summary>
    /// URIからサーバアドレス(IP or FQDN)、ポート、プロトコルを格納
    /// </summary>
    internal class ServerInfo
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Protocol { get; set; }

        public ServerInfo() { }
        public ServerInfo(string uri)
        {
            string tempServer = uri;
            string tempPort = "0";
            string tempProtocol = "";

            Match match;
            if ((match = Regex.Match(tempServer, "^.+(?=://)")).Success)
            {
                tempProtocol = match.Value;
                tempServer = tempServer.Substring(tempServer.IndexOf("://") + 3);
            }
            if ((match = Regex.Match(tempServer, @"(?<=:)\d+")).Success)
            {
                tempPort = match.Value;
                tempServer = tempServer.Substring(0, tempServer.IndexOf(":"));
            }

            this.Server = tempServer;
            this.Port = int.Parse(tempPort);
            this.Protocol = tempProtocol.ToLower();
        }

        public ServerInfo(string url, int defaultPort, string defaultProtocol) : this(url)
        {
            if (Port == 0) { this.Port = defaultPort; }
            if (string.IsNullOrEmpty(Protocol)) { this.Protocol = defaultProtocol.ToLower(); }
        }
    }
}
