using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace EnumRun.Lib
{
    internal class ProcessRange : Dictionary<string, string>
    {
        private static readonly Regex _delimiter = new Regex(@"[\-~_]");

        /*
        public string Range { get; set; }
        */

        private int[] _CurrentRange = null;


        /*        
                private int[] _Range { get; set; }

                public bool Within(int num)
                {
                    if(_Range == null)
                    {
                        string[] fields = _delimiter.Split(this.Range).Select(x => x.Trim()).ToArray();
                        if (int.TryParse(fields[0], out int startNum) && int.TryParse(fields[1], out int endNum))
                        {
                            this._Range = new int[] { startNum, endNum };
                        }
                    }
                    return num >= _Range[0] && num <= _Range[1];
                }
        */

        public void SetCurrentRange()
        {
            string key = Item.AssemblyFile;

            string currentRange = this.ContainsKey(key) ? this[key] : "";
            string[] fields = _delimiter.Split(currentRange).Select(x => x.Trim()).ToArray();
            if (int.TryParse(fields[0], out int startNum) && int.TryParse(fields[1], out int endNum))
            {
                this._CurrentRange = new int[2]
                {
                    startNum, endNum
                };
            }
        }

        public bool Within(int num)
        {
            if (_CurrentRange == null)
            {
                return false;
            }
            return num >= _CurrentRange[0] && num <= _CurrentRange[1];
        }
    }
}
