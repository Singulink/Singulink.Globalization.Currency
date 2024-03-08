namespace Singulink.Globalization.Tests.MoneySetTests;

public static partial class SubtractRange
{
    [PrefixTestClass]
    public class TImmutableMoneySet : Immutable<ImmutableMoneySet>;

    [PrefixTestClass]
    public class TImmutableSortedMoneySet : Immutable<ImmutableSortedMoneySet>;

    public class Immutable<T> where T : IImmutableMoneySet
    {
        private static readonly Money Usd100 = new(100m, "USD");
        private static readonly Money Cad50 = new(50m, "CAD");
        private static readonly Money Eur25 = new(25m, "EUR");
        private static readonly IImmutableMoneySet Set = T.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);

        [TestMethod]
        public void AllCurrenciesExist_UpdatesValues()
        {
            var resultSet = Set.SubtractRange([-Usd100, -Cad50, -Eur25]);
            resultSet.Count.ShouldBe(3);
            resultSet.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")], ignoreOrder: true);
        }

        [TestMethod]
        public void SomeNewCurrencies_UpdatesExistingAndAddsNewValues()
        {
            var resultSet = Set.SubtractRange([new(-100m, "USD"), new(-50m, "JPY"), new(-25m, "CHF"), new(0m, "CAD")]);
            resultSet.Count.ShouldBe(5);
            resultSet.ShouldBe([new(200m, "USD"), new(50m, "CAD"), new(25m, "EUR"), new(50m, "JPY"), new(25m, "CHF")], ignoreOrder: true);
        }

        [TestMethod]
        public void AllNewCurrencies_AddsValues()
        {
            var resultSet = Set.SubtractRange([new(-100m, "GBP"), new(-50m, "JPY"), new(-25m, "CHF"), default]);
            resultSet.Count.ShouldBe(6);
            resultSet.ShouldBe([new(100m, "USD"), new(50m, "CAD"), new(25m, "EUR"), new(100m, "GBP"), new(50m, "JPY"), new(25m, "CHF")], ignoreOrder: true);
        }

        [TestMethod]
        public void EmptyCollection_NoChange()
        {
            var resultSet = Set.SubtractRange([]);
            resultSet.Count.ShouldBe(3);
            resultSet.ShouldBe(Set, ignoreOrder: true);
        }

        [TestMethod]
        public void DisallowedCurrency_ThrowsArgumentException()
        {
            IEnumerable<Money> values = [new(-100m, "USD"), new(-100m, Common.CurrencyX), new(-50m, "CAD"), new(-25m, "EUR")];

            Should.Throw<ArgumentException>(() => Set.SubtractRange(values));
        }
    }
}