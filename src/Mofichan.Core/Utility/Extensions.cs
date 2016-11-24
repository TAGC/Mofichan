using System.Collections.Generic;

namespace Mofichan.Core.Utility
{
    public static class Extensions
    {
        public static TValue TryGetValueWithDefault<TValue, TKey>(
            this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            TValue value;
            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
