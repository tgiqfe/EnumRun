using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

namespace ScriptDelivery.Lib.Syslog
{
    internal class SyslogTcpSender : SyslogSender
    {
        private enum MessageTransfer
        {
            OctetCouting,
            NonTransportFraming,
        }

        private MessageTransfer _messageTransfer { get; set; }

        private TcpClient _client = null;
        private NetworkStream _stream = null;

        public SyslogTcpSender() { }
        public SyslogTcpSender(string server, bool octedCounting = true) : this(server, _defaultPort, _defaultFormat, octedCounting) { }
        public SyslogTcpSender(string server, int port, bool octetCounting = true) : this(server, port, _defaultFormat, octetCounting) { }
        public SyslogTcpSender(string server, int port, Format format, bool octetCounting = true)
        {
            this.Server = server;
            this.Port = port;
            this.Format = format;
            this._messageTransfer = octetCounting ?
                MessageTransfer.OctetCouting :
                MessageTransfer.NonTransportFraming;

            Connect();
        }

        public void Connect()
        {
            try
            {
                _client = new TcpClient(Server, Port);
                _stream = _client.GetStream();
            }
            catch
            {
                Disconnect();
            }
        }

        public void Disconnect()
        {
            if (_stream != null) { _stream.Dispose(); }
            if (_client != null) { _client.Dispose(); }
        }

        public override void Send(SyslogMessage message, Format format)
        {
            if (_stream == null)
            {
                throw new IOException("No transport stream.");
            }

            using (var ms = new MemoryStream())
            {
                byte[] datagram = format switch
                {
                    Format.RFC3164 => SyslogSerializer.GetRfc3624(message),
                    Format.RFC5424 => SyslogSerializer.GetRfc5424_ascii(message),
                    _ => null,
                };

                if (this._messageTransfer == MessageTransfer.OctetCouting)
                {
                    byte[] messageLength = Encoding.UTF8.GetBytes(datagram.Length.ToString());
                    ms.Write(messageLength, 0, messageLength.Length);
                    ms.WriteByte(32);   //  0x20 Space
                }
                ms.Write(datagram, 0, datagram.Length);

                if (this._messageTransfer == MessageTransfer.NonTransportFraming)
                {
                    ms.WriteByte(10);   //  0xA LF
                }

                _stream.Write(ms.GetBuffer(), 0, (int)ms.Length);

                //  デバッグ用
                //Console.WriteLine(Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length));
            }
        }

        public override async Task SendAsync(SyslogMessage message, Format format)
        {
            if (_stream == null)
            {
                throw new IOException("No transport stream.");
            }

            using (var ms = new MemoryStream())
            {
                byte[] datagram = format switch
                {
                    Format.RFC3164 => SyslogSerializer.GetRfc3624(message),
                    Format.RFC5424 => SyslogSerializer.GetRfc5424_ascii(message),
                    _ => null,
                };

                if (this._messageTransfer == MessageTransfer.OctetCouting)
                {
                    byte[] messageLength = Encoding.UTF8.GetBytes(datagram.Length.ToString());
                    ms.Write(messageLength, 0, messageLength.Length);
                    ms.WriteByte(32);   //  0x20 Space
                }
                ms.Write(datagram, 0, datagram.Length);

                if (this._messageTransfer == MessageTransfer.NonTransportFraming)
                {
                    ms.WriteByte(10);   //  0xA LF
                }
                await _stream.WriteAsync(ms.GetBuffer(), 0, (int)ms.Length);
            }
        }

        public override void Close()
        {
            Disconnect();
        }
    }
}
