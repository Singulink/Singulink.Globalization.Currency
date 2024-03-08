using System.Numerics;

namespace Singulink.Globalization;

#if NET7_0_OR_GREATER

/// <content>
/// Contains generic math implementation of <see cref="Money"/>.
/// </content>
partial struct Money :
    IAdditionOperators<Money, Money, Money>,
    IAdditionOperators<Money, decimal, Money>,
    IComparisonOperators<Money, Money, bool>,
    IDecrementOperators<Money>,
    IDivisionOperators<Money, decimal, Money>,
    IDivisionOperators<Money, Money, decimal>,
    IIncrementOperators<Money>,
    IMultiplyOperators<Money, decimal, Money>,
    ISubtractionOperators<Money, Money, Money>,
    ISubtractionOperators<Money, decimal, Money>,
    IUnaryNegationOperators<Money, Money>,
    IUnaryPlusOperators<Money, Money>;

#endif