using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using System.Reflection;

namespace ScriptDelivery.Maps.Requires
{
    internal class Require
    {
        [YamlMember(Alias = "mode"), Values("Mode")]
        public string Mode { get; set; }

        [YamlMember(Alias = "rule")]
        public RequireRule[] Rules { get; set; }

        public RequireMode GetRequireMode()
        {
            return ValuesAttribute.GetEnumValue<RequireMode>(
                this.GetType().GetProperty(
                    "Mode", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                this.Mode);
        }
    }
}
