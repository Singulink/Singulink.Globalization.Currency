namespace Singulink.Globalization.Tests.MoneySetTests;

public static partial class Add
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

        private static readonly IImmutableMoneySet Set = T.Create(CurrencyRegistry.Default, [Usd100, Cad50]);

        [TestMethod]
        public void AddMoney_CurrencyExists_UpdatesValue()
        {
            var resultSet = Set.Add(Usd100);

            resultSet.Count.ShouldBe(2);
            resultSet.ShouldBe([Money.Create(200m, "USD"), Cad50], ignoreOrder: true);
        }

        [TestMethod]
        public void AddMoney_NewCurrency_AddsValue()
        {
            var resultSet = Set.Add(Eur25);

            resultSet.Count.ShouldBe(3);
            resultSet.ShouldBe([Cad50, Eur25, Usd100], ignoreOrder: true);
        }

        [TestMethod]
        public void AddMoney_DefaultValue_NoChange()
        {
            var resultSet = Set.Add(default);

            resultSet.ShouldBeSameAs(Set);
        }

        [TestMethod]
        public void AddMoney_CurrencyDisallowed_ThrowsException()
        {
            var value = new Money(100, Common.CurrencyX);
            Should.Throw<ArgumentException>(() => Set.Add(value));
        }

        ///////////////////////////

        [TestMethod]
        public void AddByCurrencyCode_CurrencyExists_UpdatesValue()
        {
            var resultSet = Set.Add(100, "USD");
            resultSet.Count.ShouldBe(2);
            resultSet.ShouldBe([Cad50, new(200m, "USD")], ignoreOrder: true);
        }

        [TestMethod]
        public void AddByCurrencyCode_NewCurrency_AddsValue()
        {
            var resultSet = Set.Add(25m, "EUR");
            resultSet.Count.ShouldBe(3);
            resultSet.ShouldBe([Cad50, Eur25, Usd100], ignoreOrder: true);
        }

        [TestMethod]
        public void AddByCurrencyCode_CurrencyDisallowed_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => Set.Add(100m, Common.CurrencyX.CurrencyCode));
        }

        ///////////////////////////

        [TestMethod]
        public void AddByCurrency_CurrencyExists_UpdatesValue()
        {
            var resultSet = Set.Add(100m, Currency.Get("USD"));
            resultSet.Count.ShouldBe(2);
            resultSet.ShouldBe([Cad50, new(200m, "USD")], ignoreOrder: true);
        }

        [TestMethod]
        public void AddByCurrency_NewCurrency_AddsValue()
        {
            var resultSet = Set.Add(25m, Currency.Get("EUR"));
            resultSet.Count.ShouldBe(3);
            resultSet.ShouldBe([Cad50, Eur25, Usd100], ignoreOrder: true);
        }

        [TestMethod]
        public void AddByCurrency_CurrencyDisallowed_ThrowsArgumentException()
        {
            Set.Count.ShouldBe(2);
            Should.Throw<ArgumentException>(() => Set.Add(100m, Common.CurrencyX));
        }
    }
}