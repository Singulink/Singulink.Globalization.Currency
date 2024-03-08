namespace Singulink.Globalization.Tests.Sets;

public static partial class TrimZeroAmounts
{
    [PrefixTestClass]
    public class TMoneySet : Mutable<MoneySet>;

    [PrefixTestClass]
    public class TSortedMoneySet : Mutable<SortedMoneySet>;

    public class Mutable<T> where T : IMoneySet
    {
        private static readonly Money Usd100 = new(100m, "USD");
        private static readonly Money Cad50 = new(50m, "CAD");
        private static readonly Money Eur25 = new(25m, "EUR");
        private static readonly Money Gbp0 = new(0m, "GBP");
        private static readonly Money Jpy0 = new(0m, "JPY");

        private static readonly ImmutableArray<Money> AllValues = [Usd100, Cad50, Gbp0, Eur25, Jpy0];
        private static readonly ImmutableArray<Money> NonZeroValues = [Usd100, Cad50, Eur25];
        private static readonly ImmutableArray<Money> ZeroValues = [Gbp0, Jpy0];

        [TestMethod]
        public void SomeZeroAmounts_RemovesZeroAmountValues()
        {
            var set = T.Create(CurrencyRegistry.Default, AllValues);
            set.TrimZeroAmounts();
            set.Count.ShouldBe(3);
            set.ShouldBe(NonZeroValues, ignoreOrder: true);
        }

        [TestMethod]
        public void NoZeroAmounts_NoChange()
        {
            var set = T.Create(CurrencyRegistry.Default, NonZeroValues);
            set.TrimZeroAmounts();
            set.ShouldBe(NonZeroValues, ignoreOrder: true);
        }

        [TestMethod]
        public void AllZeroAmounts_RemovesAllValues()
        {
            var set = T.Create(CurrencyRegistry.Default, ZeroValues);
            set.TrimZeroAmounts();
            set.Count.ShouldBe(0);
            set.ShouldBe([]);
        }
    }
}