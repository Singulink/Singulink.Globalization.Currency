namespace Singulink.Globalization.Tests.BagTests;
public static class Indexers
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

        private readonly IReadOnlyMoneyBag _bag = TBag.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);

        [TestMethod]
        public void Currency_ValueExists_GetsValue()
        {
            _bag[Currency.GetCurrency("USD")].ShouldBe(Usd100);
            _bag[Currency.GetCurrency("CAD")].ShouldBe(Cad50);
            _bag[Currency.GetCurrency("EUR")].ShouldBe(Eur25);
        }

        [TestMethod]
        public void Currency_ValueDoesNotExist_GetsDefault()
        {
            _bag[Currency.GetCurrency("GBP")].IsDefault.ShouldBeTrue();
            _bag[Common.CurrencyX].IsDefault.ShouldBeTrue();
        }

        ///////////////////////////

        [TestMethod]
        public void CurrencyCode_ValueExists_GetsValue()
        {
            _bag["USD"].ShouldBe(Usd100);
            _bag["CAD"].ShouldBe(Cad50);
            _bag["EUR"].ShouldBe(Eur25);
        }

        [TestMethod]
        public void CurrencyCode_ValueDoesNotExist_GetsDefault()
        {
            _bag["GBP"].IsDefault.ShouldBeTrue();
            _bag["XXXX"].IsDefault.ShouldBeTrue();
        }
    }
}
