namespace Singulink.Globalization.Tests.BagTests;

public static partial class TrimZeroAmounts
{
    [PrefixTestClass]
    public class TMoneyBag : Mutable<MoneyBag>;

    [PrefixTestClass]
    public class TSortedMoneyBag : Mutable<SortedMoneyBag>;

    public class Mutable<TBag> where TBag : IMoneyBag
    {
        private static readonly MonetaryValue Usd100 = new(100m, "USD");
        private static readonly MonetaryValue Cad50 = new(50m, "CAD");
        private static readonly MonetaryValue Eur25 = new(25m, "EUR");
        private static readonly MonetaryValue Gbp0 = new(0m, "GBP");
        private static readonly MonetaryValue Jpy0 = new(0m, "JPY");

        private static readonly ImmutableArray<MonetaryValue> AllValues = [Usd100, Cad50, Gbp0, Eur25, Jpy0];
        private static readonly ImmutableArray<MonetaryValue> NonZeroValues = [Usd100, Cad50, Eur25];
        private static readonly ImmutableArray<MonetaryValue> ZeroValues = [Gbp0, Jpy0];

        [TestMethod]
        public void SomeZeroAmounts_RemovesZeroAmountValues()
        {
            var bag = TBag.Create(CurrencyRegistry.Default, AllValues);
            bag.TrimZeroAmounts();
            bag.Count.ShouldBe(3);
            bag.ShouldBe(NonZeroValues, ignoreOrder: true);
        }

        [TestMethod]
        public void NoZeroAmounts_NoChange()
        {
            var bag = TBag.Create(CurrencyRegistry.Default, NonZeroValues);
            bag.TrimZeroAmounts();
            bag.ShouldBe(NonZeroValues, ignoreOrder: true);
        }

        [TestMethod]
        public void AllZeroAmounts_RemovesAllValues()
        {
            var bag = TBag.Create(CurrencyRegistry.Default, ZeroValues);
            bag.TrimZeroAmounts();
            bag.Count.ShouldBe(0);
            bag.ShouldBe([]);
        }
    }
}