namespace Singulink.Globalization.Tests.BagTests;

public static partial class RemoveAll
{
    [PrefixTestClass]
    public class TMoneyBag : Mutable<MoneyBag>;

    [PrefixTestClass]
    public class TSortedMoneyBag : Mutable<SortedMoneyBag>;

    public class Mutable<TBag> where TBag : IMoneyBag
    {
        private static readonly Currency Usd = Currency.GetCurrency("USD");
        private static readonly Currency Cad = Currency.GetCurrency("CAD");
        private static readonly Currency Eur = Currency.GetCurrency("EUR");

        private static readonly MonetaryValue Usd100 = new(100m, Usd);
        private static readonly MonetaryValue Cad50 = new(50m, Cad);
        private static readonly MonetaryValue Eur25 = new(25m, Eur);

        private static readonly ImmutableArray<MonetaryValue> DefaultBagValues = [Usd100, Cad50, Eur25];

        private readonly IMoneyBag _bag = TBag.Create(CurrencyRegistry.Default, DefaultBagValues);

        [TestMethod]
        public void RemoveCurrencies_AllMatchingCurrencies_RemovesAllValues()
        {
            int removedValuesCount = _bag.RemoveAll([Usd, Cad, Eur]);

            removedValuesCount.ShouldBe(3);
            _bag.Count.ShouldBe(0);
            _bag.ShouldBe([]);
        }

        [TestMethod]
        public void RemoveCurrencies_SomeMatchingCurrencies_RemovesMatchingCurrencyValues()
        {
            int removedValuesCount = _bag.RemoveAll([Usd, Cad]);

            removedValuesCount.ShouldBe(2);
            _bag.Count.ShouldBe(1);
            _bag.ShouldBe([Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrencies_NoMatchingCurrencies_NoChange()
        {
            int removedValuesCount = _bag.RemoveAll([Currency.GetCurrency("JPY"), Currency.GetCurrency("GBP")]);

            removedValuesCount.ShouldBe(0);
            _bag.Count.ShouldBe(3);
            _bag.ShouldBe(DefaultBagValues, ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrencies_EmptyCollection_NoChange()
        {
            int removedValuesCount = _bag.RemoveAll([]);

            removedValuesCount.ShouldBe(0);
            _bag.Count.ShouldBe(3);
            _bag.ShouldBe(DefaultBagValues, ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrencies_CurrencyDisallowed_RemovesAllAllowedCurrenciesAndThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => _bag.RemoveAll([Usd, Common.CurrencyX, Common.CurrencyX, Cad]))
                .Message.ShouldBe($"The following currencies are not present in the bag's currency registry: {Common.CurrencyX} (Parameter 'currencies')");

            _bag.Count.ShouldBe(1);
            _bag.ShouldBe([Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrencies_CurrenciesDisallowed_RemovesAllAllowedCurrenciesAndThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => _bag.RemoveAll([Usd, Common.CurrencyX, Common.CurrencyY, Common.CurrencyX, Cad]))
                .Message.ShouldBe("The following currencies are not present in the bag's currency registry: " +
                    $"{Common.CurrencyX}, {Common.CurrencyY} (Parameter 'currencies')");

            _bag.Count.ShouldBe(1);
            _bag.ShouldBe([Eur25], ignoreOrder: true);
        }

        ///////////////////////////

        [TestMethod]
        public void RemoveCurrenciesByPredicate_SomeMatches_RemovesMatching()
        {
            _bag.RemoveAll(m => m.Amount > 30);
            _bag.Count.ShouldBe(1);
            _bag.ShouldBe([Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrenciesByPredicate_NoMatches_NoChange()
        {
            _bag.RemoveAll(m => m.Amount > 100);
            _bag.Count.ShouldBe(3);
            _bag.ShouldBe(DefaultBagValues, ignoreOrder: true);
        }
    }
}
