using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace ScriptDelivery.Lib.Syslog
{
    internal class SyslogSerializer
    {
        const string _nilValue = "-";

        private static readonly char[] _disallowChars = new char[] { ' ', '=', ']', '"' };

        const string _version = "1";


        /// <summary>
        /// RFC3624
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static byte[] GetRfc3624(SyslogMessage msg)
        {
            int priValue = ((int)msg.Facility * 8) + (int)msg.Severity;

            string timestamp = string.Format("{0} {1} {2}",
                msg.DateTime.ToString("MMM", CultureInfo.InvariantCulture),
                msg.DateTime.Day < 10 ?
                    " " + msg.DateTime.Day.ToString() :
                    msg.DateTime.Day.ToString(),
                msg.DateTime.ToString("HH:mm:ss"));

            string appName = string.IsNullOrEmpty(msg.AppName) ?
                msg.AppName :
                msg.AppName.Length > 32 ?
                    msg.AppName.Substring(0, 32) :
                    msg.AppName;

            string content = string.Format("<{0}>{1} {2} {3}:{4}",
                priValue, timestamp, msg.HostName, appName, msg.Message);

            //return Encoding.ASCII.GetBytes(content);
            return Encoding.UTF8.GetBytes(content);
        }

        /// <summary>
        /// RFC5242
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static byte[] GetRfc5424_ascii(SyslogMessage msg)
        {
            List<byte> list = new List<byte>();

            int priValue = ((int)msg.Facility * 8) + (int)msg.Severity;

            string timestamp = msg.DateTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffffK");

            string header = string.Format("<{0}>{1} {2} {3} {4} {5} {6}",
                priValue,
                _version,
                timestamp,
                ToAsciiField(msg.HostName, 255),
                ToAsciiField(msg.AppName, 48),
                ToAsciiField(msg.ProcId, 128),
                ToAsciiField(msg.MsgId, 32));
            list.AddRange(Encoding.ASCII.GetBytes(header));


            if (msg.StructuredDataParams == null || msg.StructuredDataParams.Length == 0)
            {
                list.AddRange(Encoding.ASCII.GetBytes(" " + _nilValue));
            }
            else
            {
                foreach (var sdp in msg.StructuredDataParams)
                {
                    list.AddRange(Encoding.ASCII.GetBytes(" [" + ToAsciiField(sdp.SdId, 32, sdName: true)));
                    if (sdp.SdParam != null)
                    {
                        var sb = new StringBuilder();
                        foreach (KeyValuePair<string, string> pair in sdp.SdParam)
                        {
                            sb.Append(string.Format(" {0}=\"{1}\"",
                                ToAsciiField(pair.Key, 32, sdName: true),
                                pair.Value == null ? "" :
                                    pair.Value.
                                        Replace("\\", "\\\\").
                                        Replace("\"", "\\\"").
                                        Replace("]", "\\]")));
                        }
                        list.AddRange(Encoding.UTF8.GetBytes(sb.ToString()));
                    }
                    list.AddRange(Encoding.ASCII.GetBytes("]"));
                }
            }

            string messageContent = msg.Message == null ?
                "" :
                " " + msg.Message;
            list.AddRange(Encoding.UTF8.GetBytes(messageContent));

            return list.ToArray();
        }

        public static byte[] GetRfc5424(SyslogMessage msg)
        {
            var sb = new StringBuilder();

            int priValue = ((int)msg.Facility * 8) + (int)msg.Severity;

            string timestamp = msg.DateTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffffK");

            sb.Append(string.Format("<{0}>{1} {2} {3} {4} {5} {6}",
                priValue,
                _version,
                timestamp,
                ToAsciiField(msg.HostName, 255),
                ToAsciiField(msg.AppName, 48),
                ToAsciiField(msg.ProcId, 128),
                ToAsciiField(msg.MsgId, 32)));

            if (msg.StructuredDataParams == null || msg.StructuredDataParams.Length == 0)
            {
                sb.Append(" " + _nilValue);
            }
            else
            {
                foreach (var sData in msg.StructuredDataParams)
                {
                    sb.Append(" [" + ToAsciiField(sData.SdId, 32, sdName: true));
                    if (sData.SdParam != null)
                    {
                        foreach (KeyValuePair<string, string> pair in sData.SdParam)
                        {
                            sb.Append(string.Format(" {0}=\"{1}\"",
                                ToAsciiField(pair.Key, 32, sdName: true),
                                pair.Value == null ? "" :
                                    pair.Value.
                                        Replace("\\", "\\\\").
                                        Replace("\"", "\\\"").
                                        Replace("]", "\\]")));
                        }
                    }
                    sb.Append("]");
                }
            }

            sb.Append(msg.Message == null ?
                "" :
                " " + msg.Message);

            return Encoding.UTF8.GetBytes(sb.ToString());
        }




        private static string ToAsciiField(string text, int maxLength, bool sdName = false)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return _nilValue;
            }
            if (text.Length > maxLength)
            {
                text = text.Substring(0, maxLength);
            }

            char[] buff = new char[255];
            int index = 0;
            foreach (char c in text)
            {
                if ((c >= 33 && c <= 126) && (!sdName || !_disallowChars.Contains(c)))
                {
                    buff[index++] = c;
                }
            }
            return new string(buff, 0, index);
        }
    }
}
