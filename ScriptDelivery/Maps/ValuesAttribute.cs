using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ScriptDelivery.Maps
{
    internal class ValuesAttribute : Attribute
    {
        private string _keyword { get; set; }

        public ValuesAttribute(string keyword)
        {
            this._keyword = keyword;
        }

        public SubCandidate[] GetCandidate()
        {
            _map ??= LoadMap();
            return _map[_keyword];     //  例外対策は無し。もし例外が発生した場合はコード見直し
        }

        public static T GetEnumValue<T>(PropertyInfo prop, string val) where T : struct
        {
            var valuesAttr = prop.GetCustomAttribute<ValuesAttribute>();
            foreach (SubCandidate candidate in valuesAttr.GetCandidate())
            {
                string enumText = candidate.Check(val);
                if (enumText != null)
                {
                    return Enum.Parse<T>(enumText, true);
                }
            }
            return default(T);
        }

        #region Value candidate map

        private static Dictionary<string, SubCandidate[]> _map = null;

        private Dictionary<string, SubCandidate[]> LoadMap()
        {
            return new Dictionary<string, SubCandidate[]>()
            {
                {
                    "Mode", new SubCandidate[]{
                        new SubCandidate("none", "no"),
                        new SubCandidate("and", "all") ,
                        new SubCandidate("or", "any") }
                },
                {
                    "Target", new SubCandidate[]{
                        new SubCandidate("none", "no"),
                        new SubCandidate("hostname", "computername", "host"),
                        new SubCandidate("ipaddress", "address", "ip", "ipaddr", "addr"),
                        new SubCandidate("env", "environment"),
                        new SubCandidate("exists", "exist"),
                        new SubCandidate("registry", "reg"),
                    }
                },
                {
                    "Match", new SubCandidate[]{
                        new SubCandidate("none", "no"),
                        new SubCandidate("equal", "equals"),
                        new SubCandidate("range", "within", "numberrange"),
                        new SubCandidate("namerange"),
                        new SubCandidate("innetwork", "insame"),
                        new SubCandidate("file", "files"),
                        new SubCandidate("directory", "folder", "directories", "folders"),
                        new SubCandidate("registry", "reg", "registories") }
                },
                {
                    "Location", new SubCandidate[]{
                        new SubCandidate("all"),
                        new SubCandidate("process", "proc"),
                        new SubCandidate("user"),
                        new SubCandidate("machine", "computer"),
                    }
                },
                {
                    "RegistryType", new SubCandidate[]{
                        new SubCandidate("reg_sz", "string"),
                        new SubCandidate("reg_dword", "dword", "int", "int32"),
                        new SubCandidate("reg_qword", "qword", "long", "int64"),
                        new SubCandidate("reg_multi_sz", "multistring", "strings", "stringarray", "array"),
                        new SubCandidate("reg_expand_sz", "expandstring", "expand_string"),
                        new SubCandidate("reg_binary", "binary", "bin"),
                        new SubCandidate("reg_none", "none", "non", "no"),
                    }
                },
            };
        }

        #endregion
    }
}
