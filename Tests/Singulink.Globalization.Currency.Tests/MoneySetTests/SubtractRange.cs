namespace Singulink.Globalization.Tests.MoneySetTests;

public static partial class SubtractRange
{
    [PrefixTestClass]
    public class Set : Mutable<MoneySet> { }

    [PrefixTestClass]
    public class SortedSet : Mutable<SortedMoneySet> { }

    [PrefixTestClass]
    public class ImmutableSet : Immutable<ImmutableMoneySet> { }

    [PrefixTestClass]
    public class ImmutableSortedSet : Immutable<ImmutableSortedMoneySet> { }
}