namespace Singulink.Globalization.Tests.BagTests;

public static class IsReadOnly
{
    [PrefixTestClass]
    public class TMoneyBag : Mutable<MoneyBag>;

    [PrefixTestClass]
    public class TSortedMoneyBag : Mutable<SortedMoneyBag>;

    [PrefixTestClass]
    public class TImmutableMoneyBag : Immutable<ImmutableMoneyBag>;

    [PrefixTestClass]
    public class TImmutableSortedMoneyBag : Immutable<ImmutableSortedMoneyBag>;

    public class Mutable<TBag> where TBag : IMoneyBag
    {
        [TestMethod]
        public void IsMutableCollection()
        {
            var bag = TBag.Create(CurrencyRegistry.Default, []);
            bag.ShouldBeMutableCollection();
        }
    }

    public class Immutable<TBag> where TBag : IImmutableMoneyBag
    {
        [TestMethod]
        public void IsReadOnlyCollection()
        {
            var bag = TBag.Create(CurrencyRegistry.Default, []);
            bag.ShouldBeReadOnlyCollection();
        }
    }
}
