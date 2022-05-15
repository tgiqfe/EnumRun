using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptDelivery.Maps
{
    internal class KeysAttribute : Attribute
    {
        private string[] _keywords { get; set; }

        public KeysAttribute(params string[] keywords)
        {
            this._keywords = keywords;
        }

        public string[] GetCandidate()
        {
            _map ??= LoadMap();

            //  例外対策は無し。もし例外が発生した場合はコード見直し
            return _keywords.Select(x => _map[x]).Aggregate((x, y) => y.Concat(x).ToArray());
        }

        #region Key candidate map

        private static Dictionary<string, string[]> _map = null;

        private Dictionary<string, string[]> LoadMap()
        {
            return new Dictionary<string, string[]>()
            {
                { "Name", new string[]{ "name" } },
                { "Start", new string[]{ "start", "start", "begin", "startname" } },
                { "End", new string[]{ "end", "finish", "fin", "endname" } },
                { "IPAddress", new string[]{ "ipaddress", "address", "ipaddr", "addr", "ipaddresses" } },
                { "NetworkAddress", new string[]{ "networkaddress", "networkaaddr", "nwaddress", "nwaddr", "network", "nw" } },
                { "Interface", new string[]{ "interface", "if" } },
                { "Key", new string[]{ "key", "keyword" } },
                { "Value", new string[]{ "value", "val" } },
                { "Path", new string[]{ "path", "pasu" } },
                { "RegistryKey", new string[]{ "registrykey", "regkey", "registry", "key" } },
                { "RegistryName", new string[]{ "registryname", "regname", "parametername", "registryparameter", "registryparam", "regparameter", "regparam"} },
                { "RegistryValue", new string[]{ "registryvalue", "regvalue", "regval" } },
                { "RegistryType", new string[]{ "registrytype", "regtype", "type" } },
                { "NoExpand", new string[]{ "noexpand", "notexpand", "expandno", "expandnot" } },
            };
        }

        #endregion
    }
}
