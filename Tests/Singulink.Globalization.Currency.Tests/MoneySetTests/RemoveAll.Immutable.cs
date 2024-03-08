namespace Singulink.Globalization.Tests.MoneySetTests;

public static partial class RemoveAll
{
    [PrefixTestClass]
    public class TImmutableMoneySet : Immutable<ImmutableMoneySet>;

    [PrefixTestClass]
    public class TImmutableSortedMoneySet : Immutable<ImmutableSortedMoneySet>;

    public class Immutable<T> where T : IImmutableMoneySet
    {
        private static readonly Currency Usd = Currency.Get("USD");
        private static readonly Currency Cad = Currency.Get("CAD");
        private static readonly Currency Eur = Currency.Get("EUR");

        private static readonly Money Usd100 = new(100m, Usd);
        private static readonly Money Cad50 = new(50m, Cad);
        private static readonly Money Eur25 = new(25m, Eur);

        private static readonly IImmutableMoneySet Set = T.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);

        [TestMethod]
        public void RemoveCurrencies_AllMatchingCurrencies_RemovesAllValues()
        {
            var resultSet = Set.RemoveAll([Usd, Cad, Eur]);

            resultSet.Count.ShouldBe(0);
        }

        [TestMethod]
        public void RemoveCurrencies_SomeMatchingCurrencies_RemovesMatchingCurrencyValues()
        {
            var resultSet = Set.RemoveAll([Usd, Cad]);

            resultSet.Count.ShouldBe(1);
            resultSet.ShouldBe([Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrencies_NoMatchingCurrencies_NoChange()
        {
            var resultSet = Set.RemoveAll([Currency.Get("JPY"), Currency.Get("GBP")]);

            resultSet.ShouldBe(Set);
        }

        [TestMethod]
        public void RemoveCurrencies_EmptyCollection_NoChange()
        {
            var resultSet = Set.RemoveAll([]);

            resultSet.ShouldBeSameAs(Set);
        }

        [TestMethod]
        public void RemoveCurrencies_CurrencyDisallowed_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => Set.RemoveAll([Common.CurrencyX]));
        }

        ///////////////////////////

        [TestMethod]
        public void RemoveCurrenciesByPredicate_SomeMatches_RemovesMatching()
        {
            var resultSet = Set.RemoveAll(m => m.Amount > 30);
            resultSet.Count.ShouldBe(1);
            resultSet.ShouldBe([Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrenciesByPredicate_NoMatches_NoChange()
        {
            var resultSet = Set.RemoveAll(m => m.Amount > 100);
            resultSet.ShouldBeSameAs(Set);
        }
    }
}
