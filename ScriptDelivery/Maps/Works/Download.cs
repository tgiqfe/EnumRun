using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using System.Diagnostics;

namespace ScriptDelivery.Maps.Works
{
    internal class Download
    {
        [YamlMember(Alias = "source")]
        public string Source { get; set; }

        [YamlMember(Alias = "destination")]
        public string Destination { get; set; }

        [YamlMember(Alias = "keep")]
        public string Keep { get; set; }

        [YamlMember(Alias = "user")]
        public string UserName { get; set; }

        [YamlMember(Alias = "password")]
        public string Password { get; set; }

        public bool GetKeep()
        {
            return this.Keep == null ?
                false :
                new string[]
                {
                    "", "0", "-", "false", "fals", "no", "not", "none", "non", "empty", "null", "否", "不", "無", "dis", "disable", "disabled"
                }.All(x => !x.Equals(this.Keep, StringComparison.OrdinalIgnoreCase));
        }
    }
}
