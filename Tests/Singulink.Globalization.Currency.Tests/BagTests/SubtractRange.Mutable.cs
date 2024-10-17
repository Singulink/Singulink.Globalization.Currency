namespace Singulink.Globalization.Tests.BagTests;

public static partial class SubtractRange
{
    [PrefixTestClass]
    public class TMoneyBag : Mutable<MoneyBag>;

    [PrefixTestClass]
    public class TSortedMoneyBag : Mutable<SortedMoneyBag>;

    public class Mutable<TBag> where TBag : IMoneyBag
    {
        private static readonly MonetaryValue Usd100 = new(100m, "USD");
        private static readonly MonetaryValue Cad50 = new(50m, "CAD");
        private static readonly MonetaryValue Eur25 = new(25m, "EUR");
        private static readonly ImmutableArray<MonetaryValue> DefaultBagValues = [Usd100, Cad50, Eur25];

        private readonly IMoneyBag _bag = TBag.Create(CurrencyRegistry.Default, DefaultBagValues);

        [TestMethod]
        public void AllCurrenciesExist_UpdatesValues()
        {
            _bag.SubtractRange([-Usd100, -Cad50, -Eur25]);
            _bag.Count.ShouldBe(3);
            _bag.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")], ignoreOrder: true);
        }

        [TestMethod]
        public void SomeNewCurrencies_UpdatesExistingAndAddsNewValues()
        {
            _bag.SubtractRange([new(-100m, "USD"), new(-50m, "JPY"), new(-25m, "CHF"), new(0m, "CAD")]);
            _bag.Count.ShouldBe(5);
            _bag.ShouldBe([new(200m, "USD"), new(50m, "CAD"), new(25m, "EUR"), new(50m, "JPY"), new(25m, "CHF")], ignoreOrder: true);
        }

        [TestMethod]
        public void AllNewCurrencies_AddsValues()
        {
            _bag.SubtractRange([new(-100m, "GBP"), new(-50m, "JPY"), new(-25m, "CHF"), default]);
            _bag.Count.ShouldBe(6);
            _bag.ShouldBe([new(100m, "USD"), new(50m, "CAD"), new(25m, "EUR"), new(100m, "GBP"), new(50m, "JPY"), new(25m, "CHF")], ignoreOrder: true);
        }

        [TestMethod]
        public void EmptyCollection_NoChange()
        {
            _bag.SubtractRange([]);
            _bag.Count.ShouldBe(3);
            _bag.ShouldBe(DefaultBagValues, ignoreOrder: true);
        }

        [TestMethod]
        public void DisallowedCurrency_ThrowsArgumentException()
        {
            IEnumerable<MonetaryValue> values = [new(-100m, "USD"), new(-100m, Common.CurrencyX), new(-100m, Common.CurrencyX), new(-50m, "CAD"), new(-25m, "EUR")];

            Should.Throw<ArgumentException>(() => _bag.SubtractRange(values))
                .Message.ShouldBe($"The following currencies are not present in the bag's currency registry: {Common.CurrencyX} (Parameter 'values')");

            _bag.Count.ShouldBe(3);
            _bag.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")], ignoreOrder: true);
        }

        [TestMethod]
        public void DisallowedCurrencies_ThrowsArgumentException()
        {
            IEnumerable<MonetaryValue> values = [new(-100m, "USD"), new(-100m, Common.CurrencyX), new(-100m, Common.CurrencyX), new(-50m, "CAD"),
            new(100m, Common.CurrencyY), new(100m, Common.CurrencyY), new(-25m, "EUR")];

            Should.Throw<ArgumentException>(() => _bag.SubtractRange(values))
                .Message.ShouldBe($"The following currencies are not present in the bag's currency registry: {Common.CurrencyX}, {Common.CurrencyY} (Parameter 'values')");

            _bag.Count.ShouldBe(3);
            _bag.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")], ignoreOrder: true);
        }
    }
}