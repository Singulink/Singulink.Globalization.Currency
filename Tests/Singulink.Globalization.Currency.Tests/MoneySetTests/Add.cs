namespace Singulink.Globalization.Tests.MoneySetTests;

public static class Add
{
    [PrefixTestClass]
    public class Set : Tests<MoneySet> { }

    [PrefixTestClass]
    public class SortedSet : Tests<SortedMoneySet> { }

    [PrefixTestClass]
    public class ImmutableSet : ImmutableTests<ImmutableMoneySet> { }

    [PrefixTestClass]
    public class ImmutableSortedSet : ImmutableTests<ImmutableSortedMoneySet> { }

    [PrefixTestClass]
    public class Tests<T> where T : IMoneySet
    {
        private static readonly Money Usd100 = new(100m, "USD");
        private static readonly Money Cad50 = new(50m, "CAD");
        private static readonly Money Eur25 = new(25m, "EUR");
        private static readonly Currency DisallowedCurrency = new("Blah blah blah", "BBB", "$$", 2);
        private static readonly ImmutableSortedMoneySet ImmutableSet = [Usd100, Cad50];

        private readonly IMoneySet _set = T.Create(CurrencyRegistry.Default, ImmutableSet);

        [TestMethod]
        public void AddMoney_CurrencyExists_UpdatesValue()
        {
            _set.Add(Usd100);
            _set.Count.ShouldBe(2);
            _set.ShouldBe([Cad50, Money.Create(200m, "USD")], ignoreOrder: true);
        }

        [TestMethod]
        public void AddMoney_NewCurrency_AddsValue()
        {
            _set.Add(Eur25);
            _set.Count.ShouldBe(3);
            _set.ShouldBe([Cad50, Eur25, Usd100], ignoreOrder: true);
        }

        [TestMethod]
        public void AddMoney_DefaultValue_NoChange()
        {
            _set.Add(default);
            _set.Count.ShouldBe(2);
            _set.ShouldBe(ImmutableSet, ignoreOrder: true);
        }

        [TestMethod]
        public void AddMoney_CurrencyDisallowed_ThrowsArgumentException()
        {
            var value = new Money(100, DisallowedCurrency);
            Should.Throw<ArgumentException>(() => _set.Add(value));
        }

        ///////////////////////////

        [TestMethod]
        public void AddByCurrencyCode_CurrencyExists_UpdatesValue()
        {
            _set.Add(100, "USD");
            _set.Count.ShouldBe(2);
            _set.ShouldBe([Cad50, new(200m, "USD")], ignoreOrder: true);
        }

        [TestMethod]
        public void AddByCurrencyCode_NewCurrency_AddsValue()
        {
            _set.Add(25m, "EUR");
            _set.Count.ShouldBe(3);
            _set.ShouldBe([Cad50, Eur25, Usd100], ignoreOrder: true);
        }

        [TestMethod]
        public void AddByCurrencyCode_CurrencyDisallowed_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => _set.Add(100m, DisallowedCurrency.CurrencyCode));
        }

        ///////////////////////////

        [TestMethod]
        public void AddByCurrency_CurrencyExists_UpdatesValue()
        {
            _set.Add(100m, Currency.Get("USD"));
            _set.Count.ShouldBe(2);
            _set.ShouldBe([Cad50, new(200m, "USD")], ignoreOrder: true);
        }

        [TestMethod]
        public void AddByCurrency_NewCurrency_AddsValue()
        {
            _set.Add(25m, Currency.Get("EUR"));
            _set.Count.ShouldBe(3);
            _set.ShouldBe([Cad50, Eur25, Usd100], ignoreOrder: true);
        }

        [TestMethod]
        public void AddByCurrency_CurrencyDisallowed_ThrowsArgumentException()
        {
            _set.Count.ShouldBe(2);
            Should.Throw<ArgumentException>(() => _set.Add(100m, DisallowedCurrency));
        }
    }

    [PrefixTestClass]
    public class ImmutableTests<T> where T : IImmutableMoneySet
    {
        private static readonly Money Usd100 = new(100m, "USD");
        private static readonly Money Cad50 = new(50m, "CAD");
        private static readonly Money Eur25 = new(25m, "EUR");
        private static readonly Currency DisallowedCurrency = new("Blah blah blah", "BBB", "$$", 2);
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
            var value = new Money(100, DisallowedCurrency);
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
            Should.Throw<ArgumentException>(() => Set.Add(100m, DisallowedCurrency.CurrencyCode));
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
            Should.Throw<ArgumentException>(() => Set.Add(100m, DisallowedCurrency));
        }
    }
}