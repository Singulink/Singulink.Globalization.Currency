namespace Singulink.Globalization.Tests.MoneySetTests;
public static class TryGetAmount
{
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
        private static readonly IReadOnlyMoneySet Set = T.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);

        // By Currency Tests

        [TestMethod]
        public void GetByCurrency_CurrencyExists_ReturnsTrueAndOutputsAmount()
        {
            Set.TryGetAmount(Currency.Get("USD"), out decimal amount).ShouldBeTrue();
            amount.ShouldBe(100m);

            Set.TryGetAmount(Currency.Get("CAD"), out amount).ShouldBeTrue();
            amount.ShouldBe(50m);

            Set.TryGetAmount(Currency.Get("EUR"), out amount).ShouldBeTrue();
            amount.ShouldBe(25m);
        }

        [TestMethod]
        public void GetByCurrency_CurrencyDoesNotExist_ReturnsFalse()
        {
            Set.TryGetAmount(Currency.Get("GBP"), out _).ShouldBeFalse();
        }

        [TestMethod]
        public void GetByCurrency_CurrencyDisallowed_ThrowsArgumentException()
        {
            var disallowedCurrency = new Currency("Disallowed Currency", "XXX", "X", 2);
            Should.Throw<ArgumentException>(() => Set.TryGetAmount(disallowedCurrency, out _));
        }

        // By Currency Code Tests

        [TestMethod]
        public void GetByCurrencyCode_CurrencyExists_ReturnsTrueAndOutputsAmount()
        {
            Set.TryGetAmount("USD", out decimal amount).ShouldBeTrue();
            amount.ShouldBe(100m);

            Set.TryGetAmount("CAD", out amount).ShouldBeTrue();
            amount.ShouldBe(50m);

            Set.TryGetAmount("EUR", out amount).ShouldBeTrue();
            amount.ShouldBe(25m);
        }

        [TestMethod]
        public void GetByCurrencyCode_CurrencyDoesNotExist_ReturnsFalse()
        {
            Set.TryGetAmount("GBP", out _).ShouldBeFalse();
        }

        [TestMethod]
        public void GetByCurrencyCode_CurrencyDisallowed_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => Set.TryGetAmount("AAA", out _));
        }
    }
}