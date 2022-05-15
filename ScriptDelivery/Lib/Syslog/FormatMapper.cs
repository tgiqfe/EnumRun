using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptDelivery.Lib.Syslog
{
    internal class FormatMapper
    {
        private static Dictionary<Format, string[]> _map = null;

        public static Format ToFormat(string text)
        {
            _map ??= new Dictionary<Format, string[]>()
            {
                { Format.RFC3164, new string[]{ "RFC3164", "3164", "3164RFC", "RFC 3164"} },
                { Format.RFC5424, new string[]{ "RFC5424", "5424", "5424RFC", "RFC 5424"} }
            };

            foreach (var pair in _map)
            {
                if (pair.Value.Any(x => x.Equals(text, StringComparison.OrdinalIgnoreCase)))
                {
                    return pair.Key;
                }
            }
            return Format.RFC3164;
        }
    }
}
