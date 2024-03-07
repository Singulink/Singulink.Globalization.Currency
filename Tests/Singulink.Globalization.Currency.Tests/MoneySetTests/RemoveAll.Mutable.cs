﻿namespace Singulink.Globalization.Tests.MoneySetTests;

public static partial class RemoveAll
{
    public class Mutable<T> where T : IMoneySet
    {
        private static readonly Currency Usd = Currency.Get("USD");
        private static readonly Currency Cad = Currency.Get("CAD");
        private static readonly Currency Eur = Currency.Get("EUR");

        private static readonly Money Usd100 = new(100m, Usd);
        private static readonly Money Cad50 = new(50m, Cad);
        private static readonly Money Eur25 = new(25m, Eur);

        private static readonly ImmutableArray<Money> DefaultSetValues = [Usd100, Cad50, Eur25];

        private readonly IMoneySet _set = T.Create(CurrencyRegistry.Default, DefaultSetValues);

        [TestMethod]
        public void RemoveCurrencies_AllMatchingCurrencies_RemovesAllValues()
        {
            int removedValuesCount = _set.RemoveAll([Usd, Cad, Eur]);

            removedValuesCount.ShouldBe(3);
            _set.Count.ShouldBe(0);
            _set.ShouldBe([]);
        }

        [TestMethod]
        public void RemoveCurrencies_SomeMatchingCurrencies_RemovesMatchingCurrencyValues()
        {
            int removedValuesCount = _set.RemoveAll([Usd, Cad]);

            removedValuesCount.ShouldBe(2);
            _set.Count.ShouldBe(1);
            _set.ShouldBe([Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrencies_NoMatchingCurrencies_NoChange()
        {
            int removedValuesCount = _set.RemoveAll([Currency.Get("JPY"), Currency.Get("GBP")]);

            removedValuesCount.ShouldBe(0);
            _set.Count.ShouldBe(3);
            _set.ShouldBe(DefaultSetValues, ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrencies_EmptyCollection_NoChange()
        {
            int removedValuesCount = _set.RemoveAll([]);

            removedValuesCount.ShouldBe(0);
            _set.Count.ShouldBe(3);
            _set.ShouldBe(DefaultSetValues, ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrencies_CurrencyDisallowed_RemovesAllAllowedCurrenciesAndThrowsArgumentException()
        {
            var disallowedCurrency = new Currency("Non-existent currency", "XXX", "X", 2);

            Should.Throw<ArgumentException>(() => _set.RemoveAll([Usd, disallowedCurrency, disallowedCurrency, Cad]))
                .Message.ShouldBe($"The following currencies are not present in the set's currency registry: {disallowedCurrency} (Parameter 'currencies')");

            _set.Count.ShouldBe(1);
            _set.ShouldBe([Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrencies_CurrenciesDisallowed_RemovesAllAllowedCurrenciesAndThrowsArgumentException()
        {
            var disallowedCurrencyX = new Currency("Non-existent currency X", "XXX", "X", 2);
            var disallowedCurrencyY = new Currency("Non-existent currency Y", "YYY", "Y", 2);

            Should.Throw<ArgumentException>(() => _set.RemoveAll([Usd, disallowedCurrencyX, disallowedCurrencyY, disallowedCurrencyX, Cad]))
                .Message.ShouldBe("The following currencies are not present in the set's currency registry: " +
                    $"{disallowedCurrencyX}, {disallowedCurrencyY} (Parameter 'currencies')");

            _set.Count.ShouldBe(1);
            _set.ShouldBe([Eur25], ignoreOrder: true);
        }

        ///////////////////////////

        [TestMethod]
        public void RemoveCurrenciesByPredicate_SomeMatches_RemovesMatching()
        {
            _set.RemoveAll(m => m.Amount > 30);
            _set.Count.ShouldBe(1);
            _set.ShouldBe([Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrenciesByPredicate_NoMatches_NoChange()
        {
            _set.RemoveAll(m => m.Amount > 100);
            _set.Count.ShouldBe(3);
            _set.ShouldBe(DefaultSetValues, ignoreOrder: true);
        }
    }
}
