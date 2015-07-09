using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon
{
    public static class StringExt
    {
        public static string Combine(this string token, params string[] parts)
        {
            return string.Join(token, parts.Where(s => !string.IsNullOrEmpty(s)));
        }
    }
}
