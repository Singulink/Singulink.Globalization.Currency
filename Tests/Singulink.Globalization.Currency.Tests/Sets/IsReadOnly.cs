namespace Singulink.Globalization.Tests.Sets;

public static class IsReadOnly
{
    [PrefixTestClass]
    public class TMoneySet : Mutable<MoneySet>;

    [PrefixTestClass]
    public class TSortedMoneySet : Mutable<SortedMoneySet>;

    [PrefixTestClass]
    public class TImmutableMoneySet : Immutable<ImmutableMoneySet>;

    [PrefixTestClass]
    public class TImmutableSortedMoneySet : Immutable<ImmutableSortedMoneySet>;

    public class Mutable<T> where T : IMoneySet
    {
        [TestMethod]
        public void IsMutableCollection()
        {
            var set = T.Create(CurrencyRegistry.Default, []);
            set.ShouldBeMutableCollection();
        }
    }

    public class Immutable<T> where T : IImmutableMoneySet
    {
        [TestMethod]
        public void IsReadOnlyCollection()
        {
            var set = T.Create(CurrencyRegistry.Default, []);
            set.ShouldBeReadOnlyCollection();
        }
    }
}
