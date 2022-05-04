using System.Text.RegularExpressions;

namespace EnumRun.Lib
{
    internal class ProcessRanges : Dictionary<string, string>
    {
        private static readonly Regex _delimiter = new Regex(@"[\-~_]");

        private int[] _CurrentRange = null;

        /// <summary>
        /// 自インスタンスの値と実行中アセンブリ名から、rangeをセット。
        /// </summary>
        /// <returns>rangeのセットへの成功/失敗</returns>
        public bool SetCurrentRange()
        {
            string key = Item.ProcessName;

            string currentRange = this.ContainsKey(key) ? this[key] : "";
            string[] fields = _delimiter.Split(currentRange).Select(x => x.Trim()).ToArray();
            if (int.TryParse(fields[0], out int startNum) && int.TryParse(fields[1], out int endNum))
            {
                this._CurrentRange = new int[2]
                {
                    startNum, endNum
                };
                return true;
            }
            return false;
        }

        /// <summary>
        /// 対象の数値がrange内かどうかの判定
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public bool Within(int num)
        {
            if (_CurrentRange == null)
            {
                bool ret = SetCurrentRange();
                if (!ret)
                {
                    return false;
                }
            }
            return num >= _CurrentRange[0] && num <= _CurrentRange[1];
        }

        /// <summary>
        /// tostring
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join(" ", this.Select(x => $"[{x.Key}]{x.Value}"));
        }
    }
}
