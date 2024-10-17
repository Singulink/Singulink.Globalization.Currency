using System.Runtime.CompilerServices;

namespace Singulink.Globalization;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <content>
/// Contains the operator implementations for <see cref="MonetaryValue"/>.
/// </content>
partial struct MonetaryValue
{
    public static bool operator ==(MonetaryValue x, MonetaryValue y) => x.Equals(y);

    public static bool operator !=(MonetaryValue x, MonetaryValue y) => !x.Equals(y);

    public static bool operator <(MonetaryValue x, MonetaryValue y)
    {
        EnsureSameCurrencyForCompare(x._currency, y._currency);
        return x._amount < y._amount;
    }

    public static bool operator >(MonetaryValue x, MonetaryValue y)
    {
        EnsureSameCurrencyForCompare(x._currency, y._currency);
        return x._amount > y._amount;
    }

    public static bool operator <=(MonetaryValue x, MonetaryValue y)
    {
        EnsureSameCurrencyForCompare(x._currency, y._currency);
        return x._amount <= y._amount;
    }

    public static bool operator >=(MonetaryValue x, MonetaryValue y)
    {
        EnsureSameCurrencyForCompare(x._currency, y._currency);
        return x._amount >= y._amount;
    }

    public static MonetaryValue operator +(MonetaryValue x, MonetaryValue y) => CreateDefaultable(x._amount + y._amount, CombineCurrenciesForAddOrSubtract(x._currency, y._currency));

    public static MonetaryValue operator +(MonetaryValue x, decimal y) => CreateDefaultable(x._amount + y, x._currency);

    public static MonetaryValue operator -(MonetaryValue x, MonetaryValue y) => CreateDefaultable(x._amount - y._amount, CombineCurrenciesForAddOrSubtract(x._currency, y._currency));

    public static MonetaryValue operator -(MonetaryValue x, decimal y) => CreateDefaultable(x._amount - y, x._currency);

    public static MonetaryValue operator *(MonetaryValue x, decimal y) => CreateDefaultable(x._amount * y, x._currency);

    public static MonetaryValue operator /(MonetaryValue x, decimal y) => CreateDefaultable(x._amount / y, x._currency);

    public static decimal operator /(MonetaryValue x, MonetaryValue y)
    {
        if (x._currency != y._currency)
        {
            void Throw() => throw new ArgumentException("Currencies must match to divide monetary values.");
            Throw();
        }

        return x._amount / y._amount;
    }

    public static MonetaryValue operator ++(MonetaryValue value) => CreateDefaultable(value.Amount + 1, value._currency);

    public static MonetaryValue operator --(MonetaryValue value) => CreateDefaultable(value.Amount - 1, value._currency);

    public static MonetaryValue operator +(MonetaryValue value) => value;

    public static MonetaryValue operator -(MonetaryValue value)
    {
        if (value.IsDefault)
            return default;

        return new(-value.Amount, value._currency);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Currency? CombineCurrenciesForAddOrSubtract(Currency? x, Currency? y)
    {
        if (x == y)
            return x;

        if (x is null)
            return y;

        if (y is not null)
        {
            static void Throw() => throw new ArgumentException("Currencies must match to add or subtract monetary values (or one of the values can be a default value that has no currency associated with it).");
            Throw();
        }

        return x;
    }
}
