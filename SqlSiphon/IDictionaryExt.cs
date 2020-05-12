using System;
using System.Collections.Generic;

namespace SqlSiphon
{
    public static class IDictionaryExt
    {
        public static ValueT Cache<KeyT, ValueT>(this IDictionary<KeyT, ValueT> dict, KeyT key, Func<KeyT, ValueT> gen)
        {
            if (dict is null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            if (gen is null)
            {
                throw new ArgumentNullException(nameof(gen));
            }

            if (!dict.ContainsKey(key))
            {
                dict.Add(key, gen(key));
            }

            return dict[key];
        }
    }
}
