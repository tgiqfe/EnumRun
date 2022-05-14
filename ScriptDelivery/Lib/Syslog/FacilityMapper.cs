using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptDelivery.Lib.Syslog
{
    internal class FacilityMapper
    {
        private static Dictionary<Facility, string[]> _map = null;

        public static Facility ToFacility(string text)
        {
            _map ??= new Dictionary<Facility, string[]>()
            {
                { Facility.KernelMessages,                       new string[]{ "KernelMessages", "0", "Kernel Messages", "kern" } },
                { Facility.UserLevelMessages,                    new string[]{ "UserLevelMessages", "1", "UserLevel Messages", "User-Level Messages", "user" } },
                { Facility.MailSystem,                           new string[]{ "MailSystem", "2", "Mail System", "mail" } },
                { Facility.SystemDaemons,                        new string[]{ "SystemDaemons", "3", "System Daemons", "daemon" } },
                { Facility.SecurityOrAuthorizationMessages_1,    new string[]{ "SecurityOrAuthorizationMessages_1", "4", "SecurityOrAuthorizationMessages1", "Security Or Authorization Messages 1", "Security/Authorization Messages 1", "auth" } },
                { Facility.MessagesGeneratedIntermallyBySyslogd, new string[]{ "MessagesGeneratedIntermallyBySyslogd", "5", "Messages Generated Intermally By Syslogd", "syslog" } },
                { Facility.LinePrinterSubsystem,                 new string[]{ "LinePrinterSubsystem", "6", "Line Printer Subsystem", "lpr" } },
                { Facility.NetworkNewsSubsystem,                 new string[]{ "NetworkNewsSubsystem", "7", "Network News Subsystem", "news" } },
                { Facility.UUCPSubsystem,                        new string[]{ "UUCPSubsystem", "8", "UUCP Subsystem", "uucp" } },
                { Facility.ClockDaemon_1,                        new string[]{ "ClockDaemon_1", "9", "ClockDaemon1", "Clock Daemon 1", "cron" } },
                { Facility.SecurityOrAuthorizationMessages_2,    new string[]{ "SecurityOrAuthorizationMessages_2", "10", "SecurityOrAuthorizationMessages2", "Security Or Authorization Messages 2", "Security/Authorization Messages 2", "authpriv" } },
                { Facility.FTPDaemon,                            new string[]{ "FTPDaemon", "11", "FTP Daemon", "ftp" } },
                { Facility.NTPSubsystem,                         new string[]{ "NTPSubsystem", "12", "NTP Subsystem" } },
                { Facility.LogAudit,                             new string[]{ "LogAudit", "13", "Log Audit" } },
                { Facility.LogAlert,                             new string[]{ "LogAlert", "14", "Log Alert" } },
                { Facility.ClockDaemon_2,                        new string[]{ "ClockDaemon_2", "15", "ClockDaemon2", "Clock Daemon 2"} },
                { Facility.LocalUse_0,                           new string[]{ "LocalUse_0", "16", "LocalUse0", "Local Use 0", "local0" } },
                { Facility.LocalUse_1,                           new string[]{ "LocalUse_1", "17", "LocalUse1", "Local Use 1", "local1" } },
                { Facility.LocalUse_2,                           new string[]{ "LocalUse_2", "18", "LocalUse2", "Local Use 2", "local2" } },
                { Facility.LocalUse_3,                           new string[]{ "LocalUse_3", "19", "LocalUse3", "Local Use 3", "local3" } },
                { Facility.LocalUse_4,                           new string[]{ "LocalUse_4", "20", "LocalUse4", "Local Use 4", "local4" } },
                { Facility.LocalUse_5,                           new string[]{ "LocalUse_5", "21", "LocalUse5", "Local Use 5", "local5" } },
                { Facility.LocalUse_6,                           new string[]{ "LocalUse_6", "22", "LocalUse6", "Local Use 6", "local6" } },
                { Facility.LocalUse_7,                           new string[]{ "LocalUse_7", "23", "LocalUse7", "Local Use 7", "local7" } },
            };

            foreach (var pair in _map)
            {
                if (pair.Value.Any(x => x.Equals(text, StringComparison.OrdinalIgnoreCase)))
                {
                    return pair.Key;
                }
            }
            return Facility.UserLevelMessages;
        }
    }
}
