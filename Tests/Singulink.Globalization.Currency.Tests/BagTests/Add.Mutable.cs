namespace Singulink.Globalization.Tests.BagTests;

public static partial class Add
{
    [PrefixTestClass]
    public class TMoneyBag : Mutable<MoneyBag>;

    [PrefixTestClass]
    public class TSortedMoneyBag : Mutable<SortedMoneyBag>;

    public class Mutable<TBag> where TBag : IMoneyBag
    {
        private static readonly MonetaryValue Cad50 = new(50m, "CAD");
        private static readonly MonetaryValue Eur25 = new(25m, "EUR");
        private static readonly MonetaryValue Usd100 = new(100m, "USD");
        private static readonly ImmutableArray<MonetaryValue> DefaultBagValues = [Cad50, Usd100];

        private readonly IMoneyBag _bag = TBag.Create(CurrencyRegistry.Default, DefaultBagValues);

        [TestMethod]
        public void AddMonetaryValue_CurrencyExists_UpdatesValue()
        {
            _bag.Add(Usd100);
            _bag.Count.ShouldBe(2);
            _bag.ShouldBe([Cad50, MonetaryValue.Create(200m, "USD")], ignoreOrder: !_bag.IsSorted);
        }

        [TestMethod]
        public void AddMonetaryValue_NewCurrency_AddsValue()
        {
            _bag.Add(Eur25);
            _bag.Count.ShouldBe(3);
            _bag.ShouldBe([Cad50, Eur25, Usd100], ignoreOrder: !_bag.IsSorted);
        }

        [TestMethod]
        public void AddMonetaryValue_DefaultValue_NoChange()
        {
            _bag.Add(default);
            _bag.Count.ShouldBe(2);
            _bag.ShouldBe(DefaultBagValues, ignoreOrder: !_bag.IsSorted);
        }

        [TestMethod]
        public void AddMonetaryValue_CurrencyDisallowed_ThrowsArgumentException()
        {
            var value = new MonetaryValue(100, Common.CurrencyX);
            Should.Throw<ArgumentException>(() => _bag.Add(value));
        }

        ///////////////////////////

        [TestMethod]
        public void AddByCurrencyCode_CurrencyExists_UpdatesValue()
        {
            _bag.Add(100, "USD");
            _bag.Count.ShouldBe(2);
            _bag.ShouldBe([Cad50, new(200m, "USD")], ignoreOrder: !_bag.IsSorted);
        }

        [TestMethod]
        public void AddByCurrencyCode_NewCurrency_AddsValue()
        {
            _bag.Add(25m, "EUR");
            _bag.Count.ShouldBe(3);
            _bag.ShouldBe([Cad50, Eur25, Usd100], ignoreOrder: !_bag.IsSorted);
        }

        [TestMethod]
        public void AddByCurrencyCode_CurrencyDisallowed_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => _bag.Add(100m, "XXXX"));
        }

        ///////////////////////////

        [TestMethod]
        public void AddByCurrency_CurrencyExists_UpdatesValue()
        {
            _bag.Add(100m, Currency.GetCurrency("USD"));
            _bag.Count.ShouldBe(2);
            _bag.ShouldBe([Cad50, new(200m, "USD")], ignoreOrder: !_bag.IsSorted);
        }

        [TestMethod]
        public void AddByCurrency_NewCurrency_AddsValue()
        {
            _bag.Add(25m, Currency.GetCurrency("EUR"));
            _bag.Count.ShouldBe(3);
            _bag.ShouldBe([Cad50, Eur25, Usd100], ignoreOrder: !_bag.IsSorted);
        }

        [TestMethod]
        public void AddByCurrency_CurrencyDisallowed_ThrowsArgumentException()
        {
            _bag.Count.ShouldBe(2);
            Should.Throw<ArgumentException>(() => _bag.Add(100m, Common.CurrencyX));
        }
    }
}