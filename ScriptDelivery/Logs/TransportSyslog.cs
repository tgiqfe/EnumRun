using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptDelivery.Lib.Syslog;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using ScriptDelivery.Lib;

namespace ScriptDelivery.Logs
{
    internal class TransportSyslog : IDisposable
    {
        public bool Enabled { get; set; }

        public SyslogSender Sender { get; set; }
        public Facility Facility { get; set; }
        public Severity Severity { get; set; }
        public string AppName { get; set; }
        public string ProcId { get; set; }
        public string MsgId { get; set; }
        public StructuredData[] StructuredDataParams { get; set; }

        public TransportSyslog() { }
        public TransportSyslog(Setting setting)
        {
            var info = new ServerInfo(setting.Syslog.Server, defaultPort: 514, defaultProtocol: "udp");
            Format format = FormatMapper.ToFormat(setting.Syslog.Format);

            if (info.Protocol == "udp")
            {
                Enabled = true;
                Sender = new SyslogUdpSender(info.Server, info.Port, format);
            }
            else
            {
                if (new TcpConnect(info.Server, info.Port).TcpConnectSuccess)
                {
                    Enabled = true;
                    Sender = setting.Syslog.SslEncrypt ?? false ?
                        new SyslogTcpSenderTLS(
                            info.Server,
                            info.Port,
                            format,
                            setting.Syslog.SslTimeout,
                            setting.Syslog.SslCertFile,
                            setting.Syslog.SslCertPassword,
                            setting.Syslog.SslCertFriendryName,
                            setting.Syslog.SslIgnoreCheck ?? false) :
                        new SyslogTcpSender(
                            info.Server,
                            info.Port,
                            format);
                }
            }
        }

        #region Send

        /*
        public void Send(string message)
        {
            Send(DateTime.Now, Facility, Severity, Environment.MachineName, AppName, ProcId, MsgId, message, StructuredDataParams);
        }

        public void Send(Severity severity, string message)
        {
            Send(DateTime.Now, Facility, severity, Environment.MachineName, AppName, ProcId, MsgId, message, StructuredDataParams);
        }

        public void Send(DateTime dt, Facility facility, Severity severity, string hostName, string appName, string procId, string msgId, string message, StructuredData[] structuredDataParams)
        {
            Sender.Send(
                new SyslogMessage(
                    dt,
                    facility,
                    severity,
                    hostName,
                    appName,
                    procId,
                    msgId,
                    message,
                    structuredDataParams));
        }

        public void Send(DateTime dt, Facility facility, LogLevel level, string hostName, string appName, string procId, string msgId, string message, StructuredData[] structuredDataParams)
        {
            Sender.Send(
                new SyslogMessage(
                    dt,
                    facility,
                    LogLevelMapper.ToSeverity(level),
                    hostName,
                    appName,
                    procId,
                    msgId,
                    message,
                    structuredDataParams));
        }

        public void Send(LogLevel level, string msgId, string message)
        {
            Send(DateTime.Now, Facility, LogLevelMapper.ToSeverity(level), Environment.MachineName, AppName, ProcId, msgId, message, StructuredDataParams);
        }
        */

        public async Task SendAsync(string message)
        {
            await SendAsync(DateTime.Now, Facility, Severity, Environment.MachineName, AppName, ProcId, MsgId, message, StructuredDataParams);
        }

        public async Task SendAsync(Severity severity, string message)
        {
            await SendAsync(DateTime.Now, Facility, severity, Environment.MachineName, AppName, ProcId, MsgId, message, StructuredDataParams);
        }

        public async Task SendAsync(DateTime dt, Facility facility, Severity severity, string hostName, string appName, string procId, string msgId, string message, StructuredData[] structuredDataParams)
        {
            await Sender.SendAsync(
                new SyslogMessage(
                    dt,
                    facility,
                    severity,
                    hostName,
                    appName,
                    procId,
                    msgId,
                    message,
                    structuredDataParams));
        }

        public async Task SendAsync(DateTime dt, Facility facility, LogLevel level, string hostName, string appName, string procId, string msgId, string message, StructuredData[] structuredDataParams)
        {
            await Sender.SendAsync(
                new SyslogMessage(
                    dt,
                    facility,
                    LogLevelMapper.ToSeverity(level),
                    hostName,
                    appName,
                    procId,
                    msgId,
                    message,
                    structuredDataParams));
        }

        public async Task SendAsync(LogLevel level, string msgId, string message)
        {
            await SendAsync(DateTime.Now, Facility, LogLevelMapper.ToSeverity(level), Environment.MachineName, AppName, ProcId, msgId, message, StructuredDataParams);
        }

        #endregion

        public void Close()
        {
            if (Sender != null)
            {
                Sender.Dispose();
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
