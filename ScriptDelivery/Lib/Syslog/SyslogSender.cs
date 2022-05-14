using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptDelivery.Lib.Syslog
{
    internal class SyslogSender : IDisposable
    {
        protected const int _defaultPort = 514;
        protected static readonly Format _defaultFormat = Format.RFC3164;
        protected const int _defaultTimeout = 3000;

        public virtual string Server { get; set; }
        public virtual int Port { get; set; }
        public virtual Format Format { get; set; }

        public virtual void Send(SyslogMessage message) { Send(message, Format); }
        public virtual void Send(SyslogMessage message, Format format) { }
        public virtual async Task SendAsync(SyslogMessage message) { await SendAsync(message, Format); }
        public virtual async Task SendAsync(SyslogMessage message, Format format) { await Task.Run(() => { }); }
        public virtual void Close() { }

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
