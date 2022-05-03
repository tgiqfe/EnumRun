using EnumRun.Log.Syslog;
using EnumRun.Log;

namespace EnumRun.Log
{
    internal class LogLevelMapper
    {
        public static Severity ToSeverity(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => Severity.Debug,
                LogLevel.Info => Severity.Informational,
                LogLevel.Attention => Severity.Notice,
                LogLevel.Warn => Severity.Warning,
                LogLevel.Error => Severity.Error,
                _ => Severity.Informational,
            };
        }

        public static LogLevel ToLogLevel(Severity severity)
        {
            return severity switch
            {
                Severity.Emergency => LogLevel.Error,
                Severity.Alert => LogLevel.Error,
                Severity.Critical => LogLevel.Error,
                Severity.Error => LogLevel.Error,
                Severity.Warning => LogLevel.Warn,
                Severity.Notice => LogLevel.Attention,
                Severity.Informational => LogLevel.Info,
                Severity.Debug => LogLevel.Debug,
                _ => LogLevel.Info,
            };
        }
    }
}
