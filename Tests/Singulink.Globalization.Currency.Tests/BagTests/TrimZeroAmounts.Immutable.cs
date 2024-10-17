namespace Singulink.Globalization.Tests.BagTests;

public static partial class TrimZeroAmounts
{
    [PrefixTestClass]
    public class TImmutableMoneyBag : Immutable<ImmutableMoneyBag>;

    [PrefixTestClass]
    public class TImmutableSortedMoneyBag : Immutable<ImmutableSortedMoneyBag>;

    public class Immutable<TBag> where TBag : IImmutableMoneyBag
    {
        private static readonly MonetaryValue Usd100 = new(100m, "USD");
        private static readonly MonetaryValue Cad50 = new(50m, "CAD");
        private static readonly MonetaryValue Eur25 = new(25m, "EUR");
        private static readonly MonetaryValue Gbp0 = new(0m, "GBP");
        private static readonly MonetaryValue Jpy0 = new(0m, "JPY");

        private static readonly IImmutableMoneyBag AllAmountsBag = TBag.Create(CurrencyRegistry.Default, [Usd100, Cad50, Gbp0, Eur25, Jpy0]);
        private static readonly IImmutableMoneyBag NoZeroAmountsBag = TBag.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);
        private static readonly IImmutableMoneyBag AllZeroAmountsBag = TBag.Create(CurrencyRegistry.Default, [Gbp0, Jpy0]);

        [TestMethod]
        public void SomeZeroAmounts_RemovesZeroAmountValues()
        {
            var resultBag = AllAmountsBag.TrimZeroAmounts();
            resultBag.Count.ShouldBe(3);
            resultBag.ShouldBe(NoZeroAmountsBag);
        }

        [TestMethod]
        public void NoZeroAmounts_NoChange()
        {
            var resultBag = NoZeroAmountsBag.TrimZeroAmounts();
            resultBag.ShouldBeSameAs(NoZeroAmountsBag);
        }

        [TestMethod]
        public void AllZeroAmounts_RemovesAllValues()
        {
            var resultBag = AllZeroAmountsBag.TrimZeroAmounts();
            resultBag.Count.ShouldBe(0);
            resultBag.ShouldBe([]);
        }
    }
}