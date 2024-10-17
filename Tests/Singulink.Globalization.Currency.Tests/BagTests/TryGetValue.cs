namespace Singulink.Globalization.Tests.BagTests;

public static class TryGetValue
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
        public void GetByCurrency_CurrencyExists_ReturnsTrueAndOutputsValue()
        {
            Bag.TryGetValue(Currency.GetCurrency("USD"), out var value).ShouldBeTrue();
            value.ShouldBe(Usd100);

            Bag.TryGetValue(Currency.GetCurrency("CAD"), out value).ShouldBeTrue();
            value.ShouldBe(Cad50);

            Bag.TryGetValue(Currency.GetCurrency("EUR"), out value).ShouldBeTrue();
            value.ShouldBe(Eur25);
        }

        [TestMethod]
        public void GetByCurrency_CurrencyDoesNotExist_ReturnsFalse()
        {
            Bag.TryGetValue(Currency.GetCurrency("GBP"), out _).ShouldBeFalse();
            Bag.TryGetValue(Common.CurrencyX, out _).ShouldBeFalse();
        }

        [TestMethod]
        public void GetByCurrencyCode_CurrencyExists_ReturnsTrueAndOutputsValue()
        {
            Bag.TryGetValue("USD", out var value).ShouldBeTrue();
            value.ShouldBe(Usd100);

            Bag.TryGetValue("CAD", out value).ShouldBeTrue();
            value.ShouldBe(Cad50);

            Bag.TryGetValue("EUR", out value).ShouldBeTrue();
            value.ShouldBe(Eur25);
        }

        [TestMethod]
        public void GetByCurrencyCode_CurrencyDoesNotExist_ReturnsFalse()
        {
            Bag.TryGetValue("GBP", out _).ShouldBeFalse();
            Bag.TryGetValue("XXXX", out _).ShouldBeFalse();
        }
    }
}