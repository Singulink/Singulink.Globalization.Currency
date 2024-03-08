namespace Singulink.Globalization.Tests.MoneySetTests;

public static partial class AddRange
{
    [PrefixTestClass]
    public class TMoneySet : Mutable<MoneySet>;

    [PrefixTestClass]
    public class TSortedMoneySet : Mutable<SortedMoneySet>;

    public class Mutable<T> where T : IMoneySet
    {
        private static readonly Money Usd100 = new(100m, "USD");
        private static readonly Money Cad50 = new(50m, "CAD");
        private static readonly Money Eur25 = new(25m, "EUR");
        private static readonly ImmutableArray<Money> DefaultSetValues = [Usd100, Cad50, Eur25];

        private readonly IMoneySet _set = T.Create(CurrencyRegistry.Default, DefaultSetValues);

        [TestMethod]
        public void AllCurrenciesExist_UpdatesValues()
        {
            _set.AddRange([Usd100, Cad50, Eur25]);
            _set.Count.ShouldBe(3);
            _set.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")], ignoreOrder: true);
        }

        [TestMethod]
        public void SomeNewCurrencies_UpdatesExistingAndAddsNewValues()
        {
            _set.AddRange([new(100m, "USD"), new(50m, "JPY"), new(25m, "CHF"), new(0m, "CAD")]);
            _set.Count.ShouldBe(5);
            _set.ShouldBe([new(200m, "USD"), new(50m, "CAD"), new(25m, "EUR"), new(50m, "JPY"), new(25m, "CHF")], ignoreOrder: true);
        }

        [TestMethod]
        public void AllNewCurrencies_AddsValues()
        {
            _set.AddRange([new(100m, "GBP"), new(50m, "JPY"), new(25m, "CHF"), default]);
            _set.Count.ShouldBe(6);
            _set.ShouldBe([new(100m, "USD"), new(50m, "CAD"), new(25m, "EUR"), new(100m, "GBP"), new(50m, "JPY"), new(25m, "CHF")], ignoreOrder: true);
        }

        [TestMethod]
        public void EmptyCollection_NoChange()
        {
            _set.AddRange([]);
            _set.Count.ShouldBe(3);
            _set.ShouldBe(DefaultSetValues, ignoreOrder: true);
        }

        [TestMethod]
        public void DisallowedCurrency_ThrowsArgumentException()
        {
            IEnumerable<Money> values = [new(100m, "USD"), new(100m, Common.CurrencyX), new(100m, Common.CurrencyX), new(50m, "CAD"), new(25m, "EUR")];

            Should.Throw<ArgumentException>(() => _set.AddRange(values))
                .Message.ShouldBe($"The following currencies are not present in the set's currency registry: {Common.CurrencyX} (Parameter 'values')");

            _set.Count.ShouldBe(3);
            _set.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")], ignoreOrder: true);
        }

        [TestMethod]
        public void DisallowedCurrencies_ThrowsArgumentException()
        {
            IEnumerable<Money> values = [new(100m, "USD"), new(100m, Common.CurrencyX), new(100m, Common.CurrencyX), new(50m, "CAD"),
            new(100m, Common.CurrencyY), new(100m, Common.CurrencyY), new(25m, "EUR")];

            Should.Throw<ArgumentException>(() => _set.AddRange(values))
                .Message.ShouldBe($"The following currencies are not present in the set's currency registry: {Common.CurrencyX}, {Common.CurrencyY} (Parameter 'values')");

            _set.Count.ShouldBe(3);
            _set.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")], ignoreOrder: true);
        }
    }
}
