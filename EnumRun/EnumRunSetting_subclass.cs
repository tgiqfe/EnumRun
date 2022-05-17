using System.Text.RegularExpressions;

namespace EnumRun
{
    internal partial class EnumRunSetting
    {
        #region Ranges

        public class ParamRanges : Dictionary<string, string>
        {
            private static readonly Regex _delimiter = new Regex(@"[\-~_]");

            private int[] _CurrentRange = null;

            /// <summary>
            /// 自インスタンスの値と実行中アセンブリ名から、rangeをセット。
            /// </summary>
            /// <returns>rangeのセットへの成功/失敗</returns>
            public bool SetCurrentRange()
            {
                string key = Item.ProcessName;

                string currentRange = this.ContainsKey(key) ? this[key] : "";
                string[] fields = _delimiter.Split(currentRange).Select(x => x.Trim()).ToArray();
                if (int.TryParse(fields[0], out int startNum) && int.TryParse(fields[1], out int endNum))
                {
                    this._CurrentRange = new int[2]
                    {
                    startNum, endNum
                    };
                    return true;
                }
                return false;
            }

            /// <summary>
            /// 対象の数値がrange内かどうかの判定
            /// </summary>
            /// <param name="num"></param>
            /// <returns></returns>
            public bool Within(int num)
            {
                if (_CurrentRange == null)
                {
                    bool ret = SetCurrentRange();
                    if (!ret)
                    {
                        return false;
                    }
                }
                return num >= _CurrentRange[0] && num <= _CurrentRange[1];
            }

            /// <summary>
            /// tostring
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return string.Join(" ", this.Select(x => $"[{x.Key}]{x.Value}"));
            }
        }

        #endregion
        #region Logstash

        /// <summary>
        /// ログ転送先サーバ(Logstash)のサーバ
        /// 記述例⇒http://192.168.10.100:8080/
        /// </summary>
        public class ParamLogstash
        {
            public string Server { get; set; }

            public override string ToString()
            {
                return string.Format("[ Server={0}] ", this.Server);
            }
        }

        #endregion
        #region Syslog

        /// <summary>
        /// ログ転送先サーバ(Syslog)のサーバ
        /// 記述例⇒udp://192.168.10.100:514
        /// </summary>
        public class ParamSyslog
        {
            public string Server { get; set; }
            public string Facility { get; set; }
            public string Format { get; set; }
            public bool? SslEncrypt { get; set; }
            public int? SslTimeout { get; set; }
            public string SslCertFile { get; set; }
            public string SslCertPassword { get; set; }
            public string SslCertFriendryName { get; set; }
            public bool? SslIgnoreCheck { get; set; }

            public override string ToString()
            {
                return string.Format(
                    "[ Server={0} Facility={1} Format={2} SslEncrypt={3} SslTimeout={4} SslCertFile={5} SslCertPassword={6} SslCertFriendryName={7} SslIgnoreCheck={8} ]",
                    this.Server,
                    this.Facility,
                    this.Format,
                    this.SslEncrypt,
                    this.SslTimeout,
                    this.SslCertFile,
                    this.SslCertPassword,
                    this.SslCertFriendryName,
                    this.SslIgnoreCheck);
            }
        }

        #endregion
        #region ScriptDelivery

        public class ParamScriptDelivery
        {
            public string[] Server { get; set; }
            public string Process { get; set; }

            public override string ToString()
            {
                return string.Format("[ Server={0} Process={1}",
                    string.Join(", ", this.Server),
                    this.Process);
            }
        }

        #endregion
    }
}
