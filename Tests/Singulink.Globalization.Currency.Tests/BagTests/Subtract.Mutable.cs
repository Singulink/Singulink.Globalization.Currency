namespace Singulink.Globalization.Tests.BagTests;

public static partial class Subtract
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
        private static readonly ImmutableArray<MonetaryValue> Values = [Usd100, Cad50];

        private readonly IMoneyBag _bag = TBag.Create(CurrencyRegistry.Default, Values);

        [TestMethod]
        public void SubtractMonetaryValue_CurrencyExists_UpdatesValue()
        {
            _bag.Subtract(-Usd100);
            _bag.Count.ShouldBe(2);
            _bag.ShouldBe([Cad50, MonetaryValue.Create(200m, "USD")], ignoreOrder: true);
        }

        [TestMethod]
        public void SubtractMonetaryValue_NewCurrency_AddsValue()
        {
            _bag.Subtract(-Eur25);
            _bag.Count.ShouldBe(3);
            _bag.ShouldBe([Cad50, Eur25, Usd100], ignoreOrder: true);
        }

        [TestMethod]
        public void SubtractMonetaryValue_DefaultValue_NoChange()
        {
            _bag.Subtract(default);
            _bag.Count.ShouldBe(2);
            _bag.ShouldBe(Values, ignoreOrder: true);
        }

        [TestMethod]
        public void SubtractMonetaryValue_CurrencyDisallowed_ThrowsArgumentException()
        {
            var value = new MonetaryValue(100, Common.DisallowedCurrency);
            Should.Throw<ArgumentException>(() => _bag.Subtract(value));
        }

        [TestMethod]
        public void SubtractByCurrencyCode_CurrencyExists_UpdatesValue()
        {
            _bag.Subtract(-100, "USD");
            _bag.Count.ShouldBe(2);
            _bag.ShouldBe([Cad50, new(200m, "USD")], ignoreOrder: true);
        }

        [TestMethod]
        public void SubtractByCurrencyCode_NewCurrency_AddsValue()
        {
            _bag.Subtract(-25m, "EUR");
            _bag.Count.ShouldBe(3);
            _bag.ShouldBe([Cad50, Eur25, Usd100], ignoreOrder: true);
        }

        [TestMethod]
        public void SubtractByCurrencyCode_CurrencyDisallowed_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => _bag.Subtract(100m, Common.DisallowedCurrency.CurrencyCode));
        }

        /////////////////////////

        [TestMethod]
        public void SubtractByCurrency_CurrencyExists_UpdatesValue()
        {
            _bag.Subtract(-100m, Currency.GetCurrency("USD"));
            _bag.Count.ShouldBe(2);
            _bag.ShouldBe([Cad50, new(200m, "USD")], ignoreOrder: true);
        }

        [TestMethod]
        public void SubtractByCurrency_NewCurrency_AddsValue()
        {
            _bag.Subtract(-25m, Currency.GetCurrency("EUR"));
            _bag.Count.ShouldBe(3);
            _bag.ShouldBe([Cad50, Eur25, Usd100], ignoreOrder: true);
        }

        [TestMethod]
        public void SubtractByCurrency_CurrencyDisallowed_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => _bag.Subtract(100m, Common.DisallowedCurrency));
        }
    }
}