namespace Singulink.Globalization.Tests.BagTests;

public static partial class RemoveAll
{
    [PrefixTestClass]
    public class TImmutableMoneyBag : Immutable<ImmutableMoneyBag>;

    [PrefixTestClass]
    public class TImmutableSortedMoneyBag : Immutable<ImmutableSortedMoneyBag>;

    public class Immutable<TBag> where TBag : IImmutableMoneyBag
    {
        private static readonly Currency Usd = Currency.GetCurrency("USD");
        private static readonly Currency Cad = Currency.GetCurrency("CAD");
        private static readonly Currency Eur = Currency.GetCurrency("EUR");

        private static readonly MonetaryValue Usd100 = new(100m, Usd);
        private static readonly MonetaryValue Cad50 = new(50m, Cad);
        private static readonly MonetaryValue Eur25 = new(25m, Eur);

        private static readonly IImmutableMoneyBag Bag = TBag.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);

        [TestMethod]
        public void RemoveCurrencies_AllMatchingCurrencies_RemovesAllValues()
        {
            var resultBag = Bag.RemoveAll([Usd, Cad, Eur]);

            resultBag.Count.ShouldBe(0);
        }

        [TestMethod]
        public void RemoveCurrencies_SomeMatchingCurrencies_RemovesMatchingCurrencyValues()
        {
            var resultBag = Bag.RemoveAll([Usd, Cad]);

            resultBag.Count.ShouldBe(1);
            resultBag.ShouldBe([Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrencies_NoMatchingCurrencies_NoChange()
        {
            var resultBag = Bag.RemoveAll([Currency.GetCurrency("JPY"), Currency.GetCurrency("GBP")]);

            resultBag.ShouldBe(Bag);
        }

        [TestMethod]
        public void RemoveCurrencies_EmptyCollection_NoChange()
        {
            var resultBag = Bag.RemoveAll([]);

            resultBag.ShouldBeSameAs(Bag);
        }

        [TestMethod]
        public void RemoveCurrencies_CurrencyDisallowed_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => Bag.RemoveAll([Common.CurrencyX]));
        }

        ///////////////////////////

        [TestMethod]
        public void RemoveCurrenciesByPredicate_SomeMatches_RemovesMatching()
        {
            var resultBag = Bag.RemoveAll(m => m.Amount > 30);
            resultBag.Count.ShouldBe(1);
            resultBag.ShouldBe([Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrenciesByPredicate_NoMatches_NoChange()
        {
            var resultBag = Bag.RemoveAll(m => m.Amount > 100);
            resultBag.ShouldBeSameAs(Bag);
        }
    }
}
