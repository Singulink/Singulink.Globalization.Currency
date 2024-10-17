using System.Text;

namespace Singulink.Globalization;

/// <summary>
/// Provides a bag of static methods for converting between money collection types.
/// </summary>
public static class MoneyCollectionExtensions
{
    /// <summary>
    /// Copies the values in the source bag to a new <see cref="MoneyBag"/> that uses the same registry as the source bag.
    /// </summary>
    public static MoneyBag ToMoneyBag(this IReadOnlyMoneyBag source)
    {
        return new MoneyBag(source.Registry, source, false);
    }

    /// <summary>
    /// Copies the values in the source bag to a new <see cref="MoneyBag"/> that uses the specified registry.
    /// </summary>
    public static MoneyBag ToMoneyBag(this IReadOnlyMoneyBag source, CurrencyRegistry registry)
    {
        return new MoneyBag(registry, source, registry == source.Registry);
    }

    /// <summary>
    /// Copies the values in the source collection to a new <see cref="MoneyBag"/> that uses the specified registry.
    /// </summary>
    public static MoneyBag ToMoneyBag(this IEnumerable<MonetaryValue> source, CurrencyRegistry registry)
    {
        return new MoneyBag(registry, source);
    }

    /// <summary>
    /// Copies the values in the source bag to a new <see cref="SortedMoneyBag"/> that uses the same registry as the source bag.
    /// </summary>
    public static SortedMoneyBag ToSortedMoneyBag(this IReadOnlyMoneyBag source)
    {
        return new SortedMoneyBag(source.Registry, source, false);
    }

    /// <summary>
    /// Copies the values in the source bag to a new <see cref="SortedMoneyBag"/> that uses the specified registry.
    /// </summary>
    public static SortedMoneyBag ToSortedMoneyBag(this IReadOnlyMoneyBag source, CurrencyRegistry registry)
    {
        return new SortedMoneyBag(registry, source, registry == source.Registry);
    }

    /// <summary>
    /// Copies the values in the source collection to a new <see cref="SortedMoneyBag"/> that uses the specified registry.
    /// </summary>
    public static SortedMoneyBag ToSortedMoneyBag(this IEnumerable<MonetaryValue> source, CurrencyRegistry registry)
    {
        return new SortedMoneyBag(registry, source);
    }

    /// <summary>
    /// Copies the values in the source bag to a new <see cref="ImmutableMoneyBag"/> that uses the same registry as the source bag.
    /// </summary>
    public static ImmutableMoneyBag ToImmutableMoneyBag(this IReadOnlyMoneyBag source)
    {
        return new ImmutableMoneyBag(source.Registry, source, false);
    }

    /// <summary>
    /// Copies the values in the source bag to a new <see cref="ImmutableMoneyBag"/> that uses the specified registry.
    /// </summary>
    public static ImmutableMoneyBag ToImmutableMoneyBag(this IReadOnlyMoneyBag source, CurrencyRegistry registry)
    {
        return new ImmutableMoneyBag(registry, source, registry == source.Registry);
    }

    /// <summary>
    /// Copies the values in the source collection to a new <see cref="ImmutableMoneyBag"/> that uses the specified registry.
    /// </summary>
    public static ImmutableMoneyBag ToImmutableMoneyBag(this IEnumerable<MonetaryValue> source, CurrencyRegistry registry)
    {
        return ImmutableMoneyBag.CreateRange(registry, source);
    }

    /// <summary>
    /// Copies the values in the source bag to a new <see cref="ImmutableSortedMoneyBag"/> that uses the same registry as the source bag.
    /// </summary>
    public static ImmutableSortedMoneyBag ToImmutableSortedMoneyBag(this IReadOnlyMoneyBag source)
    {
        return new ImmutableSortedMoneyBag(source.Registry, source, false);
    }

    /// <summary>
    /// Copies the values in the source bag to a new <see cref="ImmutableSortedMoneyBag"/> that uses the specified registry.
    /// </summary>
    public static ImmutableSortedMoneyBag ToImmutableSortedMoneyBag(this IReadOnlyMoneyBag source, CurrencyRegistry registry)
    {
        return new ImmutableSortedMoneyBag(registry, source, registry == source.Registry);
    }

    /// <summary>
    /// Copies the values in the source collection to a new <see cref="ImmutableSortedMoneyBag"/> that uses the specified registry.
    /// </summary>
    public static ImmutableSortedMoneyBag ToImmutableSortedMoneyBag(this IEnumerable<MonetaryValue> source, CurrencyRegistry registry)
    {
        return ImmutableSortedMoneyBag.CreateRange(registry, source);
    }

    /// <summary>
    /// Returns a string representation of the monetary values this bag contains.
    /// </summary>
    /// <param name="bag">The bag.</param>
    /// <param name="format">The format to use for each monetary value. See <see cref="MonetaryValue.ToString(string?, IFormatProvider?)"/> for valid monetary formats.
    /// Prepend the desired monetary format with the <c>!</c> character to ignore zero amount values.</param>
    /// <param name="provider">The format provider that will be used to obtain number format information. This should be a <see cref="CultureInfo"/>
    /// instance for formats that depend on the culture, otherwise the current culture is used.</param>
    internal static string ToString(IReadOnlyMoneyBag bag, string? format, IFormatProvider? provider)
    {
        bool ignoreZeroAmounts = false;
        int estimatedCount = bag.Count;

        if (format is ['!', ..])
        {
            format = format[1..];
            ignoreZeroAmounts = true;

            if (estimatedCount > 8)
                estimatedCount /= 2; // Assume that half of the values will be ignored
        }

        if (estimatedCount is 0)
            return string.Empty;

        var sb = new StringBuilder(estimatedCount * 12);
        bool first = true;

#if NET
        StringBuilder.AppendInterpolatedStringHandler sbh = new(0, 0, sb, provider);
#endif

        foreach (var value in bag)
        {
            if (ignoreZeroAmounts && value.Amount is 0)
                continue;

            if (first)
                first = false;
            else
                sb.Append(", ");
#if NET
            sbh.AppendFormatted(value, format);
#else
            sb.Append(value.ToString(format, provider));
#endif
        }

        return sb.ToString();
    }
}
