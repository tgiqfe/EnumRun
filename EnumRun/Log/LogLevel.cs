using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnumRun.Log
{
    public enum LogLevel
    {
        Debug = -1,     //  デバッグレベル
        Info = 0,       //  通常の情報レベル。
        Warn = 2,       //  警告レベル
        Error = 3,      //  エラーレベル
    }
}
