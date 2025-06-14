namespace Singulink.Globalization.Tests.BagTests;

public static partial class SubtractRange
{
    [PrefixTestClass]
    public class TImmutableMoneyBag : Immutable<ImmutableMoneyBag>;

    [PrefixTestClass]
    public class TImmutableSortedMoneyBag : Immutable<ImmutableSortedMoneyBag>;

    public class Immutable<TBag> where TBag : IImmutableMoneyBag
    {
        private static readonly MonetaryValue Usd100 = new(100m, "USD");
        private static readonly MonetaryValue Cad50 = new(50m, "CAD");
        private static readonly MonetaryValue Eur25 = new(25m, "EUR");
        private static readonly IImmutableMoneyBag Bag = TBag.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);

        [TestMethod]
        public void AllCurrenciesExist_UpdatesValues()
        {
            var resultBag = Bag.SubtractRange([-Usd100, -Cad50, -Eur25]);
            resultBag.Count.ShouldBe(3);
            resultBag.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")], ignoreOrder: true);
        }

        [TestMethod]
        public void SomeNewCurrencies_UpdatesExistingAndAddsNewValues()
        {
            var resultBag = Bag.SubtractRange([new(-100m, "USD"), new(-50m, "JPY"), new(-25m, "CHF"), new(0m, "CAD")]);
            resultBag.Count.ShouldBe(5);
            resultBag.ShouldBe([new(200m, "USD"), new(50m, "CAD"), new(25m, "EUR"), new(50m, "JPY"), new(25m, "CHF")], ignoreOrder: true);
        }

        [TestMethod]
        public void AllNewCurrencies_AddsValues()
        {
            var resultBag = Bag.SubtractRange([new(-100m, "GBP"), new(-50m, "JPY"), new(-25m, "CHF"), default]);
            resultBag.Count.ShouldBe(6);
            resultBag.ShouldBe([new(100m, "USD"), new(50m, "CAD"), new(25m, "EUR"), new(100m, "GBP"), new(50m, "JPY"), new(25m, "CHF")], ignoreOrder: true);
        }

        [TestMethod]
        public void EmptyCollection_NoChange()
        {
            var resultBag = Bag.SubtractRange([]);
            resultBag.Count.ShouldBe(3);
            resultBag.ShouldBe(Bag, ignoreOrder: true);
        }

        [TestMethod]
        public void DisallowedCurrency_ThrowsArgumentException()
        {
            IEnumerable<MonetaryValue> values = [new(-100m, "USD"), new(-100m, Common.CurrencyX), new(-50m, "CAD"), new(-25m, "EUR")];

            Should.Throw<ArgumentException>(() => Bag.SubtractRange(values));
        }
    }
}