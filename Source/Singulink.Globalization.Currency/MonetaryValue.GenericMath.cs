using System.Numerics;

namespace Singulink.Globalization;

#if NET

/// <content>
/// Contains generic math implementation of <see cref="MonetaryValue"/>.
/// </content>
partial struct MonetaryValue :
    IAdditionOperators<MonetaryValue, MonetaryValue, MonetaryValue>,
    IAdditionOperators<MonetaryValue, decimal, MonetaryValue>,
    IAdditiveIdentity<MonetaryValue, MonetaryValue>,
    IComparisonOperators<MonetaryValue, MonetaryValue, bool>,
    IDecrementOperators<MonetaryValue>,
    IDivisionOperators<MonetaryValue, decimal, MonetaryValue>,
    IDivisionOperators<MonetaryValue, MonetaryValue, decimal>,
    IIncrementOperators<MonetaryValue>,
    IMultiplyOperators<MonetaryValue, decimal, MonetaryValue>,
    ISubtractionOperators<MonetaryValue, MonetaryValue, MonetaryValue>,
    ISubtractionOperators<MonetaryValue, decimal, MonetaryValue>,
    IUnaryNegationOperators<MonetaryValue, MonetaryValue>,
    IUnaryPlusOperators<MonetaryValue, MonetaryValue>
{
    /// <summary>
    /// Gets the additive identity, which is the <see cref="Default"/> value. Adding it to any value returns that value unchanged, in the same currency and
    /// with the same amount, so generic code that starts a sum from the additive identity works with values in any currency.
    /// </summary>
    static MonetaryValue IAdditiveIdentity<MonetaryValue, MonetaryValue>.AdditiveIdentity => default;
}

#endif