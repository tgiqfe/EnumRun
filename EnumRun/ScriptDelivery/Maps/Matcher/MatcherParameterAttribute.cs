using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnumRun.ScriptDelivery.Maps.Matcher
{
    internal class MatcherParameterAttribute : Attribute
    {
        /// <summary>
        /// 必須パラメータ
        /// </summary>
        public bool Mandatory { get; set; }

        /// <summary>
        /// 0以上が設定されているパラメータのどれかが設定されている必要有り。
        /// 同じ値のパラメータが複数ある場合は、それら全てを含む必要有り。
        /// </summary>
        public int MandatoryAny { get; set; }

        /// <summary>
        /// 環境変数を展開するかどうか
        /// </summary>
        public bool Expand { get; set; }

        /// <summary>
        /// 符号付数値を無効化するかどうか
        /// </summary>
        public bool Unsigned { get; set; }

        /// <summary>
        /// 配列 or Dictionaryの場合の、各パラメータの区切り文字
        /// </summary>
        public char Delimiter { get; set; }

        /// <summary>
        /// Dictionaryの場合の、keyとvalueを区切る文字
        /// </summary>
        public char EqualSign { get; set; }
    }
}
