#if NET

namespace Singulink.Globalization;

/// <summary>
/// Provides a static method that creates a money bag of the implementing type, for creating bags in generic code, i.e. <c>TBag.Create(registry, values)</c>
/// in a method that has a <c>where TBag : IMoneyBagFactory&lt;TBag&gt;</c> constraint.
/// </summary>
/// <typeparam name="TSelf">The money bag type that implements this interface.</typeparam>
/// <remarks>
/// <para>
/// <see cref="MoneyBag"/>, <see cref="SortedMoneyBag"/>, <see cref="ImmutableMoneyBag"/> and <see cref="ImmutableSortedMoneyBag"/> implement this interface. It
/// is separate from the bag interfaces so that <see cref="IReadOnlyMoneyBag"/>, <see cref="IMoneyBag"/> and <see cref="IImmutableMoneyBag"/> have no static
/// abstract members and can be used as type arguments.</para>
/// </remarks>
public interface IMoneyBagFactory<TSelf>
    where TSelf : IReadOnlyMoneyBag, IMoneyBagFactory<TSelf>
{
    /// <summary>
    /// Creates a bag that uses the specified currency registry and adds all the specified values.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public static abstract TSelf Create(CurrencyRegistry registry, IEnumerable<MonetaryValue> values);
}

#endif
