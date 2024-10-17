using System.Numerics;

namespace Singulink.Globalization;

#if NET

/// <content>
/// Contains generic math implementation of <see cref="MonetaryValue"/>.
/// </content>
partial struct MonetaryValue :
    IAdditionOperators<MonetaryValue, MonetaryValue, MonetaryValue>,
    IAdditionOperators<MonetaryValue, decimal, MonetaryValue>,
    IComparisonOperators<MonetaryValue, MonetaryValue, bool>,
    IDecrementOperators<MonetaryValue>,
    IDivisionOperators<MonetaryValue, decimal, MonetaryValue>,
    IDivisionOperators<MonetaryValue, MonetaryValue, decimal>,
    IIncrementOperators<MonetaryValue>,
    IMultiplyOperators<MonetaryValue, decimal, MonetaryValue>,
    ISubtractionOperators<MonetaryValue, MonetaryValue, MonetaryValue>,
    ISubtractionOperators<MonetaryValue, decimal, MonetaryValue>,
    IUnaryNegationOperators<MonetaryValue, MonetaryValue>,
    IUnaryPlusOperators<MonetaryValue, MonetaryValue>;

#endif