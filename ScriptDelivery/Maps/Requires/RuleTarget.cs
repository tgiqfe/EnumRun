using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptDelivery.Maps.Requires
{
    internal enum RuleTarget
    {
        None = 0,
        HostName = 1,
        IPAddress = 2,
        Env = 3,
        Exists = 4,
        Registy = 5,
    }
}
