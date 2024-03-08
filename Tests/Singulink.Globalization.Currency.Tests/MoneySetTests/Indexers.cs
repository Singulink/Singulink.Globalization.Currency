namespace Singulink.Globalization.Tests.MoneySetTests;
public static class Indexers
{
    [PrefixTestClass]
    public class TMoneySet : Tests<MoneySet>;

    [PrefixTestClass]
    public class TSortedMoneySet : Tests<SortedMoneySet>;

    [PrefixTestClass]
    public class TImmutableMoneySet : Tests<ImmutableMoneySet>;

    [PrefixTestClass]
    public class TImmutableSortedMoneySet : Tests<ImmutableSortedMoneySet>;

    public class Tests<T> where T : IReadOnlyMoneySet
    {
        private static readonly Money Usd100 = new(100m, "USD");
        private static readonly Money Cad50 = new(50m, "CAD");
        private static readonly Money Eur25 = new(25m, "EUR");

        private readonly IReadOnlyMoneySet _set = T.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);

        [TestMethod]
        public void Currency_ValueExists_GetsValue()
        {
            _set[Currency.Get("USD")].ShouldBe(Usd100);
            _set[Currency.Get("CAD")].ShouldBe(Cad50);
            _set[Currency.Get("EUR")].ShouldBe(Eur25);
        }

        [TestMethod]
        public void Currency_ValueDoesNotExist_GetsDefault()
        {
            _set[Currency.Get("GBP")].IsDefault.ShouldBeTrue();
            _set[Common.CurrencyX].IsDefault.ShouldBeTrue();
        }

        ///////////////////////////

        [TestMethod]
        public void CurrencyCode_ValueExists_GetsValue()
        {
            _set["USD"].ShouldBe(Usd100);
            _set["CAD"].ShouldBe(Cad50);
            _set["EUR"].ShouldBe(Eur25);
        }

        [TestMethod]
        public void CurrencyCode_ValueDoesNotExist_GetsDefault()
        {
            _set["GBP"].IsDefault.ShouldBeTrue();
            _set["XXXX"].IsDefault.ShouldBeTrue();
        }
    }
}
