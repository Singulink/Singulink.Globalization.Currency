using Microsoft.Win32;

namespace Singulink.Globalization;

/// <summary>
/// Provides a set of static methods for converting between money collection types.
/// </summary>
public static class MoneyCollectionExtensions
{
    /// <summary>
    /// Copies the values in the source set to a new <see cref="MoneySet"/> that uses the same registry as the source set.
    /// </summary>
    public static MoneySet ToMoneySet(this IReadOnlyMoneySet source)
    {
        return new MoneySet(source.Registry, source, false);
    }

    /// <summary>
    /// Copies the values in the source set to a new <see cref="MoneySet"/> that uses the specified registry.
    /// </summary>
    public static MoneySet ToMoneySet(this IReadOnlyMoneySet source, CurrencyRegistry registry)
    {
        return new MoneySet(registry, source, registry == source.Registry);
    }

    /// <summary>
    /// Copies the values in the source collection to a new <see cref="MoneySet"/> that uses the specified registry.
    /// </summary>
    public static MoneySet ToMoneySet(this IEnumerable<Money> source, CurrencyRegistry registry)
    {
        return new MoneySet(registry, source);
    }

    /// <summary>
    /// Copies the values in the source set to a new <see cref="SortedMoneySet"/> that uses the same registry as the source set.
    /// </summary>
    public static SortedMoneySet ToSortedMoneySet(this IReadOnlyMoneySet source)
    {
        return new SortedMoneySet(source.Registry, source, false);
    }

    /// <summary>
    /// Copies the values in the source set to a new <see cref="SortedMoneySet"/> that uses the specified registry.
    /// </summary>
    public static SortedMoneySet ToSortedMoneySet(this IReadOnlyMoneySet source, CurrencyRegistry registry)
    {
        return new SortedMoneySet(registry, source, registry == source.Registry);
    }

    /// <summary>
    /// Copies the values in the source collection to a new <see cref="SortedMoneySet"/> that uses the specified registry.
    /// </summary>
    public static SortedMoneySet ToSortedMoneySet(this IEnumerable<Money> source, CurrencyRegistry registry)
    {
        return new SortedMoneySet(registry, source);
    }

    /// <summary>
    /// Copies the values in the source set to a new <see cref="ImmutableMoneySet"/> that uses the same registry as the source set.
    /// </summary>
    public static ImmutableMoneySet ToImmutableMoneySet(this IReadOnlyMoneySet source)
    {
        return new ImmutableMoneySet(source.Registry, source, false);
    }

    /// <summary>
    /// Copies the values in the source set to a new <see cref="ImmutableMoneySet"/> that uses the specified registry.
    /// </summary>
    public static ImmutableMoneySet ToImmutableMoneySet(this IReadOnlyMoneySet source, CurrencyRegistry registry)
    {
        return new ImmutableMoneySet(registry, source, registry == source.Registry);
    }

    /// <summary>
    /// Copies the values in the source collection to a new <see cref="ImmutableMoneySet"/> that uses the specified registry.
    /// </summary>
    public static ImmutableMoneySet ToImmutableMoneySet(this IEnumerable<Money> source, CurrencyRegistry registry)
    {
        return ImmutableMoneySet.CreateRange(registry, source);
    }

    /// <summary>
    /// Copies the values in the source set to a new <see cref="ImmutableSortedMoneySet"/> that uses the same registry as the source set.
    /// </summary>
    public static ImmutableSortedMoneySet ToImmutableSortedMoneySet(this IReadOnlyMoneySet source)
    {
        return new ImmutableSortedMoneySet(source.Registry, source, false);
    }

    /// <summary>
    /// Copies the values in the source set to a new <see cref="ImmutableSortedMoneySet"/> that uses the specified registry.
    /// </summary>
    public static ImmutableSortedMoneySet ToImmutableSortedMoneySet(this IReadOnlyMoneySet source, CurrencyRegistry registry)
    {
        return new ImmutableSortedMoneySet(registry, source, registry == source.Registry);
    }

    /// <summary>
    /// Copies the values in the source collection to a new <see cref="ImmutableSortedMoneySet"/> that uses the specified registry.
    /// </summary>
    public static ImmutableSortedMoneySet ToImmutableSortedMoneySet(this IEnumerable<Money> source, CurrencyRegistry registry)
    {
        return ImmutableSortedMoneySet.CreateRange(registry, source);
    }
}
