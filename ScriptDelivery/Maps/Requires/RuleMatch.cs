using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptDelivery.Maps.Requires
{
    internal enum RuleMatch
    {
        None = -1,          //  マッチ条件無し。
        Equal = 0,          //  完全一致判定、ワイルドカードを使用してContains,StartsWith,EndsWith
        Range = 1,          //  末尾数字部分のみの範囲判定。IPアドレスの場合は第四オクテットのみ
        NameRange = 2,      //  ホスト名の前半部分と数字の範囲で判定
        InNetwork = 3,      //  IPアドレスがネットワークアドレスの範囲かどうか
        File = 4,           //  ファイルに関する判定
        Directory = 5,      //  フォルダーに関する判定
        Registry = 6        //  レジストリキー、レジストリ値に関する判定
    }
}
