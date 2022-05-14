using ScriptDelivery.Lib.Syslog;
using ScriptDelivery.Logs;

namespace ScriptDelivery.Logs
{
    internal class LogLevelMapper
    {
        private static Dictionary<LogLevel, string[]> _map = null;

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

        public static LogLevel ToLogLevel(string text)
        {
            _map ??= new Dictionary<LogLevel, string[]>()
            {
                { LogLevel.Debug,     new string[] { "Debug", "dbg" } },
                { LogLevel.Info,      new string[] { "Info", "inf", "Information", "Informational" } },
                { LogLevel.Attention, new string[] { "Attention", "att", "Notice" } },
                { LogLevel.Warn,      new string[] { "Warn", "Warning", "wan" } },
                { LogLevel.Error,     new string[] { "Error", "err", "Critical", "Alert", "Emergency"} },
            };

            foreach (var pair in _map)
            {
                if (pair.Value.Any(x => x.Equals(text, StringComparison.OrdinalIgnoreCase)))
                {
                    return pair.Key;
                }
            }
            return LogLevel.Info;
        }
    }
}
