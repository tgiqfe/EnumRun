using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptDelivery.Maps.Works;
using ScriptDelivery.Maps.Requires;

namespace ScriptDelivery.Maps
{
    internal class Mapping
    {
        public string Name { get; set; }

        public Require Require { get; set; }

        public Work Work { get; set; }
    }
}
