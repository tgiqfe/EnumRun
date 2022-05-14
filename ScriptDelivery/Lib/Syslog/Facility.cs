using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptDelivery.Lib.Syslog
{
    internal enum Facility
    {
        KernelMessages = 0,
        UserLevelMessages = 1,
        MailSystem = 2,
        SystemDaemons = 3,
        SecurityOrAuthorizationMessages_1 = 4,
        MessagesGeneratedIntermallyBySyslogd = 5,
        LinePrinterSubsystem = 6,
        NetworkNewsSubsystem = 7,
        UUCPSubsystem = 8,
        ClockDaemon_1 = 9,
        SecurityOrAuthorizationMessages_2 = 10,
        FTPDaemon = 11,
        NTPSubsystem = 12,
        LogAudit = 13,
        LogAlert = 14,
        ClockDaemon_2 = 15,
        LocalUse_0 = 16,
        LocalUse_1 = 17,
        LocalUse_2 = 18,
        LocalUse_3 = 19,
        LocalUse_4 = 20,
        LocalUse_5 = 21,
        LocalUse_6 = 22,
        LocalUse_7 = 23
    }
}
