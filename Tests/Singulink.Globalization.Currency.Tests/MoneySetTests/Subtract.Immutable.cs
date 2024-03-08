namespace Singulink.Globalization.Tests.MoneySetTests;

public static partial class Subtract
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
        private static readonly Currency DisallowedCurrency = new("Blah blah blah", "BBB", "$$", 2);

        private static readonly IImmutableMoneySet Set = T.Create(CurrencyRegistry.Default, [Usd100, Cad50]);

        [TestMethod]
        public void SubtractMoney_CurrencyExists_UpdatesValue()
        {
            var resultSet = Set.Subtract(-Usd100);
            resultSet.Count.ShouldBe(2);
            resultSet.ShouldBe([Cad50, Money.Create(200m, "USD")], ignoreOrder: true);
        }

        [TestMethod]
        public void SubtractMoney_NewCurrency_AddsValue()
        {
            var resultSet = Set.Subtract(-Eur25);
            resultSet.Count.ShouldBe(3);
            resultSet.ShouldBe([Cad50, Eur25, Usd100], ignoreOrder: true);
        }

        [TestMethod]
        public void SubtractMoney_DefaultValue_NoChange()
        {
            var resultSet = Set.Subtract(default);
            resultSet.Count.ShouldBe(2);
            resultSet.ShouldBe(Set, ignoreOrder: true);
        }

        [TestMethod]
        public void SubtractMoney_CurrencyDisallowed_ThrowsArgumentException()
        {
            var value = new Money(100, DisallowedCurrency);
            Should.Throw<ArgumentException>(() => Set.Subtract(value));
        }

        /////////////////////////

        [TestMethod]
        public void SubtractByCurrencyCode_CurrencyExists_UpdatesValue()
        {
            var resultSet = Set.Subtract(-100, "USD");
            resultSet.Count.ShouldBe(2);
            resultSet.ShouldBe([Cad50, new(200m, "USD")], ignoreOrder: true);
        }

        [TestMethod]
        public void SubtractByCurrencyCode_NewCurrency_AddsValue()
        {
            var resultSet = Set.Subtract(-25m, "EUR");
            resultSet.Count.ShouldBe(3);
            resultSet.ShouldBe([Cad50, Eur25, Usd100], ignoreOrder: true);
        }

        [TestMethod]
        public void SubtractByCurrencyCode_CurrencyDisallowed_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => Set.Subtract(100m, DisallowedCurrency.CurrencyCode));
        }

        /////////////////////////

        [TestMethod]
        public void SubtractByCurrency_CurrencyExists_UpdatesValue()
        {
            var resultSet = Set.Subtract(-100m, Currency.Get("USD"));
            resultSet.Count.ShouldBe(2);
            resultSet.ShouldBe([Cad50, new(200m, "USD")], ignoreOrder: true);
        }

        [TestMethod]
        public void SubtractByCurrency_NewCurrency_AddsValue()
        {
            var resultSet = Set.Subtract(-25m, Currency.Get("EUR"));
            resultSet.Count.ShouldBe(3);
            resultSet.ShouldBe([Cad50, Eur25, Usd100], ignoreOrder: true);
        }

        [TestMethod]
        public void SubtractByCurrency_CurrencyDisallowed_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => Set.Subtract(100m, DisallowedCurrency));
        }
    }
}