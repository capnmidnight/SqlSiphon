using System.Linq;

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
