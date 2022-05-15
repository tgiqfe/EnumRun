using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ScriptDelivery.Lib.Syslog
{
    internal class SyslogUdpSender : SyslogSender
    {
        private UdpClient _client = null;

        public SyslogUdpSender() { }
        public SyslogUdpSender(string server) : this(server, _defaultPort, _defaultFormat) { }
        public SyslogUdpSender(string server, int port) : this(server, port, _defaultFormat) { }
        public SyslogUdpSender(string server, int port, Format format)
        {
            this.Server = server;
            this.Port = port;
            this.Format = format;
        }

        public override void Send(SyslogMessage message, Format format)
        {
            _client ??= new UdpClient(Server, Port);

            byte[] datagram = format switch
            {
                Format.RFC3164 => SyslogSerializer.GetRfc3624(message),
                Format.RFC5424 => SyslogSerializer.GetRfc5424(message),
                _ => null,
            };
            _client.Send(datagram, datagram.Length);

            //  デバッグ用
            //Console.WriteLine(Encoding.UTF8.GetString(datagram));
        }

        public override async Task SendAsync(SyslogMessage message, Format format)
        {
            _client ??= new UdpClient(Server, Port);

            byte[] datagram = format switch
            {
                Format.RFC3164 => SyslogSerializer.GetRfc3624(message),
                Format.RFC5424 => SyslogSerializer.GetRfc5424(message),
                _ => null,
            };
            await _client.SendAsync(datagram, datagram.Length);

            //  デバッグ用
            //Console.WriteLine(Encoding.UTF8.GetString(datagram));
        }

        public override void Close()
        {
            if (_client != null)
            {
                _client.Dispose();
            }
        }
    }
}
