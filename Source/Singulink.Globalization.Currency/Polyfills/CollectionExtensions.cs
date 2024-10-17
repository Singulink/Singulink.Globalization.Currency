#if NETSTANDARD

namespace Singulink.Globalization.Polyfills;

internal static class CollectionExtensions
{
    public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> d, TKey key, TValue value)
    {
        if (d.ContainsKey(key))
            return false;

        d.Add(key, value);
        return true;
    }
}

#endif