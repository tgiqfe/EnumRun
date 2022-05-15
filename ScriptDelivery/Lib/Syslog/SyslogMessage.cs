using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ScriptDelivery.Lib.Syslog
{
    internal class SyslogMessage
    {
        public DateTime DateTime { get; set; }
        public Facility Facility { get; set; }
        public Severity Severity { get; set; }
        public string HostName { get; set; }
        public string AppName { get; set; }
        public string ProcId { get; set; }
        public string MsgId { get; set; }
        public string Message { get; set; }
        public StructuredData[] StructuredDataParams { get; set; }

        public SyslogMessage() { }

        public SyslogMessage(
            Facility facility,
            Severity severity,
            string appName,
            string message) :
            this(DateTime.Now, facility, severity, Environment.MachineName, appName, Process.GetCurrentProcess().Id.ToString(), "", message)
        { }

        public SyslogMessage(
            DateTime dt,
            Facility facility,
            Severity severity,
            string hostName,
            string appName,
            string procId,
            string msgId,
            string message,
            params StructuredData[] StructuredDatas)
        {
            this.DateTime = dt;
            this.Facility = facility;
            this.Severity = severity;
            this.HostName = hostName;
            this.AppName = appName;
            this.ProcId = procId;
            this.MsgId = msgId;
            this.Message = message;
            this.StructuredDataParams = StructuredDatas;
        }
    }
}
