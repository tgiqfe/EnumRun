using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace EnumRun.Lib
{
    internal class ProcessRange
    {
        public string Range { get; set; }

        private Regex _delimiter = new Regex(@"[\-~_]");
        private int[] _Range { get; set; }

        /*
        public int GetStartNumber()
        {
            if (_Range == null) { SplitRange(); }
            return _Range[0];
        }

        public int GetEndNumber()
        {
            if (_Range == null) { SplitRange(); }
            return _Range[1];
        }

        private void SplitRange()
        {
            string[] fields = _delimiter.Split(this.Range).Select(x => x.Trim()).ToArray();
            if (int.TryParse(fields[0], out int startNum) && int.TryParse(fields[1], out int endNum))
            {
                this._Range = new int[] { startNum, endNum };
            }
        }
        */

        public bool Within(int num)
        {
            //if (_Range == null) { SplitRange(); }
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
    }
}
