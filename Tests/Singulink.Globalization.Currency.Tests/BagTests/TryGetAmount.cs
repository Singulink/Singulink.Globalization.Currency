namespace Singulink.Globalization.Tests.BagTests;

public static class TryGetAmount
{
    [PrefixTestClass]
    public class TMoneyBag : Tests<MoneyBag>;

    [PrefixTestClass]
    public class TSortedMoneyBag : Tests<SortedMoneyBag>;

    [PrefixTestClass]
    public class TImmutableMoneyBag : Tests<ImmutableMoneyBag>;

    [PrefixTestClass]
    public class TImmutableSortedMoneyBag : Tests<ImmutableSortedMoneyBag>;

    public class Tests<TBag> where TBag : IReadOnlyMoneyBag
    {
        private static readonly MonetaryValue Usd100 = new(100m, "USD");
        private static readonly MonetaryValue Cad50 = new(50m, "CAD");
        private static readonly MonetaryValue Eur25 = new(25m, "EUR");
        private static readonly IReadOnlyMoneyBag Bag = TBag.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);

        [TestMethod]
        public void GetByCurrency_CurrencyExists_ReturnsTrueAndOutputsAmount()
        {
            Bag.TryGetAmount(Currency.GetCurrency("USD"), out decimal amount).ShouldBeTrue();
            amount.ShouldBe(100m);

            Bag.TryGetAmount(Currency.GetCurrency("CAD"), out amount).ShouldBeTrue();
            amount.ShouldBe(50m);

            Bag.TryGetAmount(Currency.GetCurrency("EUR"), out amount).ShouldBeTrue();
            amount.ShouldBe(25m);
        }

        [TestMethod]
        public void GetByCurrency_CurrencyDoesNotExist_ReturnsFalse()
        {
            Bag.TryGetAmount(Currency.GetCurrency("GBP"), out _).ShouldBeFalse();
            Bag.TryGetAmount(Common.CurrencyX, out _).ShouldBeFalse();
        }

        ///////////////////////////

        [TestMethod]
        public void GetByCurrencyCode_CurrencyExists_ReturnsTrueAndOutputsAmount()
        {
            Bag.TryGetAmount("USD", out decimal amount).ShouldBeTrue();
            amount.ShouldBe(100m);

            Bag.TryGetAmount("CAD", out amount).ShouldBeTrue();
            amount.ShouldBe(50m);

            Bag.TryGetAmount("EUR", out amount).ShouldBeTrue();
            amount.ShouldBe(25m);
        }

        [TestMethod]
        public void GetByCurrencyCode_CurrencyDoesNotExist_ReturnsFalse()
        {
            Bag.TryGetAmount("GBP", out _).ShouldBeFalse();
            Bag.TryGetAmount("XXXX", out _).ShouldBeFalse();
        }
    }
}