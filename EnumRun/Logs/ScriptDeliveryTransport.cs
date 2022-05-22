using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace EnumRun.Logs
{
    internal class ScriptDeliveryTransport : IDisposable
    {
        public bool Enabled { get; set; }

        private HttpClient _client = null;

        public ScriptDeliveryTransport() { }




        public void Close()
        {
          
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
