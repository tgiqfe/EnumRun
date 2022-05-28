using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using EnumRun.Lib;
using ScriptDelivery.Maps.Requires;
using ScriptDelivery.Maps.Works;
using ScriptDelivery;
using ScriptDelivery.Maps;
using EnumRun.Logs.ProcessLog;
using EnumRun.Logs;

namespace EnumRun.ScriptDelivery.Maps.Matcher
{
    internal class MatcherBase
    {
        public static MatcherBase Activate(RuleTarget target)
        {
            return target switch
            {
                RuleTarget.None => null,
                RuleTarget.HostName => new HostNameMatcher(),
                RuleTarget.IPAddress => new IPAddressMatcher(),
                RuleTarget.Env => new EnvMatcher(),
                RuleTarget.Exists => new ExistsMatcher(),
                RuleTarget.Registy => new RegistryMatcher(),
                _ => null,
            };
        }

        protected ProcessLogger2 _logger = null;

        public void SetLogger(ProcessLogger2 logger)
        {
            string logTitle = "SetLogger";

            this._logger = logger;
            _logger.Write(LogLevel.Debug, logTitle, "Matcher [{0}]", this.GetType().Name);
        }

        /// <summary>
        /// 文字列情報から各パラメータをセット
        /// </summary>
        /// <param name="param"></param>
        public void SetParam(Dictionary<string, string> param)
        {
            string logTitle = "SetParam";

            if (param == null) { return; }

            //  大文字小文字を無視するDictionaryに変換
            var dictionary = new Dictionary<string, string>(param, StringComparer.OrdinalIgnoreCase);

            var props = this.GetType().GetProperties(
                BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            foreach (var prop in props)
            {
                var paramAttr = prop.GetCustomAttribute<MatcherParameterAttribute>();
                if (paramAttr == null) { continue; }

                var keys = prop.GetCustomAttribute<KeysAttribute>();
                string matchKey = keys?.GetCandidate().FirstOrDefault(x => dictionary.ContainsKey(x));
                if (matchKey != null)
                {
                    string matchValue = dictionary[matchKey];
                    Type type = prop.PropertyType;

                    if (type == typeof(string))
                    {
                        _logger.Write(LogLevel.Debug, logTitle, "Parameter type: string, key={0}, value={1}", matchKey, matchValue);
                        SetString(prop, matchValue, paramAttr.Expand);
                    }
                    else if (type == typeof(bool?))
                    {
                        _logger.Write(LogLevel.Debug, logTitle, "Parameter type: nullable bool, key={0}, value={1}", matchKey, matchValue);
                        SetBool(prop, matchValue);
                    }
                    else if (type == typeof(int?))
                    {
                        _logger.Write(LogLevel.Debug, logTitle, "Parameter type: nullable int, key={0}, value={1}", matchKey, matchValue);
                        SetInt(prop, matchValue, paramAttr.Unsigned);
                    }
                    else if (type == typeof(DateTime?))
                    {
                        _logger.Write(LogLevel.Debug, logTitle, "Parameter type: nullable DataTime, key={0}, value={1}", matchKey, matchValue);
                        SetDateTime(prop, matchValue);
                    }
                    else if (type == typeof(string[]))
                    {
                        _logger.Write(LogLevel.Debug, logTitle, "Parameter type: string array, key={0}, value={1}", matchKey, matchValue);
                        SetStrings(prop, matchValue, paramAttr.Delimiter, paramAttr.Expand);
                    }
                    else if (type == typeof(Dictionary<string, string>))
                    {
                        _logger.Write(LogLevel.Debug, logTitle, "Parameter type: Dictionary<string, string>, key={0}, value={1}", matchKey, matchValue);
                        SetDictionary(prop, matchValue, paramAttr.Delimiter, paramAttr.EqualSign, paramAttr.Expand);
                    }
                    else if ((type = Nullable.GetUnderlyingType(type)).IsEnum)
                    {
                        _logger.Write(LogLevel.Debug, logTitle, "Parameter type: nullable enum, key={0}, value={1}", matchKey, matchValue);
                        SetEnum(prop, matchValue, type);
                    }
                }
            }
        }

        #region Set paremter pivate methods

        private void SetString(PropertyInfo prop, string val, bool expand)
        {
            if (expand)
            {
                val = ExpandEnvironment(val);
            }
            prop.SetValue(this, val);
        }

        private void SetBool(PropertyInfo prop, string val)
        {
            if (!string.IsNullOrEmpty(val))
            {
                prop.SetValue(this, !BooleanCandidate.IsNullableFalse(val));
            }
        }

        private void SetInt(PropertyInfo prop, string val, bool unsigned)
        {
            if (int.TryParse(val, out int tempInt))
            {
                if (!unsigned || tempInt >= 0)
                {
                    prop.SetValue(this, tempInt);
                }
            }
        }

        private void SetDateTime(PropertyInfo prop, string val)
        {
            prop.SetValue(this, DateTime.TryParse(val, out DateTime dt) ? dt : null);
        }

        private void SetStrings(PropertyInfo prop, string val, char pDelimiter, bool expand)
        {
            char delimiter = val.Contains(pDelimiter) ? pDelimiter : '\n';
            string[] array = val.Split(delimiter).
                Select(x => x.Trim()).
                Select(x => expand ? ExpandEnvironment(x) : x).ToArray().
                ToArray();
            prop.SetValue(this, array);
        }

        private void SetDictionary(PropertyInfo prop, string val, char pDelimiter, char equalSign, bool expand)
        {
            var dictionary = new Dictionary<string, string>();

            char delimiter = val.Contains(pDelimiter) ? pDelimiter : '\n';
            val.Split(delimiter).
                Select(x => x.Trim()).
                Where(x => x.Contains(equalSign)).
                Select(x => expand ? ExpandEnvironment(x) : x).ToArray().
                ToList().
                ForEach(x =>
                {
                    string itemKey = x.Substring(0, x.IndexOf(equalSign)).Trim();
                    string itemValue = x.Substring(x.IndexOf(equalSign) + 1).Trim();
                    dictionary[itemKey] = itemValue;
                });
            prop.SetValue(this, dictionary);
        }

        private void SetEnum(PropertyInfo prop, string val, Type type)
        {
            var values = prop.GetCustomAttribute<ValuesAttribute>();
            if (values != null)
            {
                foreach (SubCandidate candidate in values.GetCandidate())
                {
                    string enumText = candidate.Check(val);
                    if (enumText != null)
                    {
                        prop.SetValue(this, Enum.Parse(type, enumText, true));
                        break;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// 必要なパラメータがセットされているかどうかをチェック
        /// </summary>
        public bool CheckParam()
        {
            string logTitle = "CheckParam";

            bool ret = true;

            var props = this.GetType().GetProperties(
                BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            var mAny = new Dictionary<int, bool>();
            foreach (var prop in props)
            {
                var paramAttr = prop.GetCustomAttribute<MatcherParameterAttribute>();
                if (paramAttr == null) { continue; }

                object val = prop.GetValue(this);
                if (paramAttr.Mandatory)
                {
                    ret &= IsDefined(val);
                }
                if (paramAttr.MandatoryAny > 0)
                {
                    if (!mAny.ContainsKey(paramAttr.MandatoryAny))
                    {
                        mAny[paramAttr.MandatoryAny] = true;
                    }
                    mAny[paramAttr.MandatoryAny] &= IsDefined(val);
                }
            }
            if (mAny.Count > 0) { ret &= mAny.Any(x => x.Value); }

            _logger.Write(ret ? LogLevel.Debug : LogLevel.Attention,
                logTitle,
                "Parameter check => {0}", ret ? "Success" : "Failed");

            return ret;
        }

        /// <summary>
        /// 値がセット済みかどうか
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected bool IsDefined(object obj)
        {
            return obj switch
            {
                string s => s != null,
                string[] ar => (ar?.Length > 0),
                Dictionary<string, string> dic => (dic?.Count > 0),
                _ => obj != null,
            };
        }

        protected string ExpandEnvironment(string text)
        {
            for (int i = 0; i < 5 && text.Contains("%"); i++)
            {
                text = Environment.ExpandEnvironmentVariables(text);
            }
            return text;
        }

        public virtual bool IsMatch(RuleMatch matchType) { return false; }
    }
}
