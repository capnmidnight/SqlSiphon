using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlSiphon
{
    public static class IEnumerabledExt
    {
        public static Dictionary<TKey, TSource[]> ToHash<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.GroupBy(keySelector).ToDictionary(g => g.Key, g => g.ToArray());
        }
    }
}
