namespace Singulink.Globalization.Tests.MoneySetTests;

public static partial class Add
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
        private static readonly ImmutableArray<Money> DefaultSetValues = [Usd100, Cad50];

        private readonly IMoneySet _set = T.Create(CurrencyRegistry.Default, DefaultSetValues);

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
            _set.ShouldBe(DefaultSetValues, ignoreOrder: true);
        }

        [TestMethod]
        public void AddMoney_CurrencyDisallowed_ThrowsArgumentException()
        {
            var value = new Money(100, Common.CurrencyX);
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
            Should.Throw<ArgumentException>(() => _set.Add(100m, "XXXX"));
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
            Should.Throw<ArgumentException>(() => _set.Add(100m, Common.CurrencyX));
        }
    }
}