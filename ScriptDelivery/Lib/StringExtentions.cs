using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptDelivery.Lib
{
    internal static class StringExtentions
    {
        public static System.Text.RegularExpressions.Regex GetWildcardPattern(this string s)
        {
            string patternString = System.Text.RegularExpressions.Regex.Replace(s, ".",
            x =>
            {
                string y = x.Value;
                if (y.Equals("?")) { return "."; }
                else if (y.Equals("*")) { return ".*"; }
                else { return System.Text.RegularExpressions.Regex.Escape(y); }
            });
            if (!patternString.StartsWith("*")) { patternString = "^" + patternString; }
            if (!patternString.EndsWith("*")) { patternString = patternString + "$"; }
            return new System.Text.RegularExpressions.Regex(
                patternString, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
    }
}
