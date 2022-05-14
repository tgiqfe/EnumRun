using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Security.Authentication;
using System.Threading;

namespace ScriptDelivery.Lib.Syslog
{
    internal class SyslogTcpSenderTLS : SyslogSender
    {
        private enum MessageTransfer
        {
            OctetCouting,
            NonTransportFraming,
        }

        public int Timeout { get; set; }
        public string CertFile { get; set; }
        public string CertPassword { get; set; }
        public string CertFriendryName { get; set; }
        public bool IgnoreCheck { get; set; }

        private TcpClient _client = null;
        private SslStream _stream = null;
        private MessageTransfer _messageTransfer { get; set; }

        public SyslogTcpSenderTLS() { }
        public SyslogTcpSenderTLS(string server, bool octetCouting = true) : this(server, _defaultPort, _defaultFormat, _defaultTimeout, null, null, null, false, octetCouting) { }
        public SyslogTcpSenderTLS(string server, int port, bool octetCouting = true) : this(server, port, _defaultFormat, _defaultTimeout, null, null, null, false, octetCouting) { }
        public SyslogTcpSenderTLS(string server, int port, Format format, bool octetCounting = true) : this(server, port, format, _defaultTimeout, null, null, null, false, octetCounting) { }
        public SyslogTcpSenderTLS(string server, int port, Format format, int? timeout, string certFile, string certPassword, string certFriendryName, bool ignoreCheck, bool octetCouting = true)
        {
            this.Server = server;
            this.Port = port;
            this.Format = format;
            this.Timeout = timeout ?? _defaultTimeout;
            this.CertFile = certFile;
            this.CertPassword = certPassword;
            this.CertFriendryName = certFriendryName;
            this.IgnoreCheck = ignoreCheck;
            this._messageTransfer = octetCouting ?
                MessageTransfer.OctetCouting :
                MessageTransfer.NonTransportFraming;

            Connect();
        }

        public void Connect()
        {
            try
            {
                _client = new TcpClient(Server, Port);
                _stream = new SslStream(_client.GetStream(), false, RemoteCertificateValidationCallback)
                {
                    ReadTimeout = Timeout,
                    WriteTimeout = Timeout
                };
                _stream.AuthenticateAsClient(
                    Server,
                    GetCollection(),
                    SslProtocols.Tls12 | SslProtocols.Tls13,
                    false);

                if (!_stream.IsEncrypted)
                {
                    throw new SecurityException("Could not establish an encrypted connection.");
                }
            }
            catch
            {
                Disconnect();
            }
        }

        private X509Certificate2Collection GetCollection()
        {
            var collection = new X509Certificate2Collection();
            if (!string.IsNullOrEmpty(this.CertFile) && !string.IsNullOrEmpty(this.CertPassword) && File.Exists(this.CertFile))
            {
                collection.Add(new X509Certificate2(this.CertFile, this.CertPassword));
            }
            else if (!string.IsNullOrEmpty(this.CertFriendryName))
            {
                var myCert = new X509Store(StoreName.My, StoreLocation.CurrentUser, OpenFlags.ReadOnly).Certificates.
                    FirstOrDefault(x => x.FriendlyName == this.CertFriendryName);
                if (myCert == null)
                {
                    new X509Store(StoreName.My, StoreLocation.LocalMachine, OpenFlags.ReadOnly).Certificates.
                        FirstOrDefault(x => x.FriendlyName == this.CertFriendryName);
                }
                if (myCert != null)
                {
                    collection.Add(myCert);
                }
            }

            return collection;
        }

        private bool RemoteCertificateValidationCallback(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            //  デバッグ用
            //Debug_CertPrint(certificate);

            if (this.IgnoreCheck || sslPolicyErrors == SslPolicyErrors.None)
            {
                //  Successful verification of server certificate.
                return true;
            }
            else
            {
                //  ChainStatus returned a non-empty array.
                if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) == SslPolicyErrors.RemoteCertificateChainErrors) { }
                //  Certificate names do not match.
                if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) == SslPolicyErrors.RemoteCertificateNameMismatch) { }
                //  Certificate not available.
                if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) == SslPolicyErrors.RemoteCertificateNotAvailable) { }

                return false;
            }
        }

        private void Debug_CertPrint(X509Certificate certificate)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("Subject={0}", certificate.Subject);
            Console.WriteLine("Issuer={0}", certificate.Issuer);
            Console.WriteLine("Format={0}", certificate.GetFormat());
            Console.WriteLine("ExpirationDate={0}", certificate.GetExpirationDateString());
            Console.WriteLine("EffectiveDate={0}", certificate.GetEffectiveDateString());
            Console.WriteLine("KeyAlgorithm={0}", certificate.GetKeyAlgorithm());
            Console.WriteLine("PublicKey={0}", certificate.GetPublicKeyString());
            Console.WriteLine("SerialNumber={0}", certificate.GetSerialNumberString());
            Console.WriteLine("===========================================");
        }

        public void Disconnect()
        {
            if (_stream != null) { _stream.Dispose(); }
            if (_client != null) { _client.Dispose(); }
        }

        public override void Send(SyslogMessage message, Format format)
        {
            if (_stream == null || !_stream.CanWrite)
            {
                //  No transport stream.
                return;
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
                    byte[] messageLength = Encoding.ASCII.GetBytes(datagram.Length.ToString());
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

                _stream.Flush();
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
                    byte[] messageLength = Encoding.ASCII.GetBytes(datagram.Length.ToString());
                    ms.Write(messageLength, 0, messageLength.Length);
                    ms.WriteByte(32);   //  0x20 Space
                }
                ms.Write(datagram, 0, datagram.Length);

                if (this._messageTransfer == MessageTransfer.NonTransportFraming)
                {
                    ms.WriteByte(10);   //  0xA LF
                }
                await _stream.WriteAsync(ms.GetBuffer(), 0, (int)ms.Length);
                await _stream.FlushAsync();
            }
        }

        public override void Close()
        {
            Disconnect();
        }
    }
}
