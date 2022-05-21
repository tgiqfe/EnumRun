using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnumRun.ScriptDelivery
{
    internal class DownloadSmb
    {
        public string TargetPath { get; set; }
        public string ShareName { get; set; }
        public string Destination { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool Overwrite { get; set; }
    }
}
