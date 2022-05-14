using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptDelivery.Lib
{
    internal class BooleanCandidate
    {
        private static readonly string[] _falseCandidate = new string[]
        {
            "", "0", "-", "false", "fals", "no", "not", "none", "non", "empty", "null", "否", "不", "無", "dis", "disable", "disabled"
        };

        private static readonly string[] _trueCandidate = new string[]
        {
            "1", "true", "tru", "成", "是", "可", "有", "en", "enable", "enabled"
        };

        public static bool IsFalse(string val)
        {
            string lowerVal = val.ToLower();
            return _falseCandidate.Any(x => x.Equals(lowerVal));
        }

        public static bool? IsNullableFalse(string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                return null;
            }
            string lowerVal = val.ToLower();
            return _falseCandidate.Any(x => x.Equals(lowerVal));
        }

        public static bool IsTrue(string val)
        {
            string lowerVal = val.ToLower();
            return _trueCandidate.Any(x => x.Equals(lowerVal));
        }

        public static bool? IsNullableTrue(string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                return null;
            }
            string lowerVal = val.ToLower();
            return _trueCandidate.Any(x => x.Equals(lowerVal));
        }
    }
}
