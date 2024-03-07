namespace Singulink.Globalization.Tests.MoneySetTests;

public static class TryGetValue
{
    [PrefixTestClass]
    public class Set : Tests<MoneySet> { }

    [PrefixTestClass]
    public class SortedSet : Tests<SortedMoneySet> { }

    [PrefixTestClass]
    public class ImmutableSet : Tests<ImmutableMoneySet> { }

    [PrefixTestClass]
    public class ImmutableSortedSet : Tests<ImmutableSortedMoneySet> { }

    public class Tests<T> where T : IReadOnlyMoneySet
    {
        private static readonly Money Usd100 = new(100m, "USD");
        private static readonly Money Cad50 = new(50m, "CAD");
        private static readonly Money Eur25 = new(25m, "EUR");

        private static readonly IReadOnlyMoneySet _set = T.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);

        [TestMethod]
        public void GetByCurrency_CurrencyExists_ReturnsTrueAndOutputsValue()
        {
            _set.TryGetValue(Currency.Get("USD"), out var value).ShouldBeTrue();
            value.ShouldBe(Usd100);

            _set.TryGetValue(Currency.Get("CAD"), out value).ShouldBeTrue();
            value.ShouldBe(Cad50);

            _set.TryGetValue(Currency.Get("EUR"), out value).ShouldBeTrue();
            value.ShouldBe(Eur25);
        }

        [TestMethod]
        public void GetByCurrency_CurrencyDoesNotExist_ReturnsFalse()
        {
            var newCurrency = new Currency("New Currency", "XXXX", "X", 2);

            _set.TryGetValue(Currency.Get("GBP"), out _).ShouldBeFalse();
            _set.TryGetValue(newCurrency, out _).ShouldBeFalse();
        }

        ///////////////////////////

        [TestMethod]
        public void GetByCurrencyCode_CurrencyExists_ReturnsTrueAndOutputsValue()
        {
            _set.TryGetValue("USD", out var value).ShouldBeTrue();
            value.ShouldBe(Usd100);

            _set.TryGetValue("CAD", out value).ShouldBeTrue();
            value.ShouldBe(Cad50);

            _set.TryGetValue("EUR", out value).ShouldBeTrue();
            value.ShouldBe(Eur25);
        }

        [TestMethod]
        public void GetByCurrencyCode_CurrencyDoesNotExist_ReturnsFalse()
        {
            _set.TryGetValue("GBP", out _).ShouldBeFalse();
            _set.TryGetValue("XXXX", out _).ShouldBeFalse();
        }
    }
}