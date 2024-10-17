namespace Singulink.Globalization.Tests.BagTests;

public static partial class SetAmount
{
    [PrefixTestClass]
    public class TMoneyBag : Mutable<MoneyBag>;

    [PrefixTestClass]
    public class TSortedMoneyBag : Mutable<SortedMoneyBag>;

    public class Mutable<TBag> where TBag : IMoneyBag
    {
        private static readonly Currency Usd = Currency.GetCurrency("USD");
        private static readonly Currency Aud = Currency.GetCurrency("AUD");

        private static readonly MonetaryValue Usd0 = new(0m, "USD");
        private static readonly MonetaryValue Usd100 = new(100m, "USD");
        private static readonly MonetaryValue Cad50 = new(50m, "CAD");
        private static readonly MonetaryValue Eur25 = new(25m, "EUR");
        private static readonly MonetaryValue Aud75 = new(75m, "AUD");

        private static readonly ImmutableArray<MonetaryValue> DefaultBagValues = [Usd100, Cad50, Eur25];

        private readonly IMoneyBag _bag = TBag.Create(CurrencyRegistry.Default, DefaultBagValues);

        [TestMethod]
        public void SetByCurrencyCode_CurrencyExists_UpdatesValue()
        {
            _bag.SetAmount(200m, "USD");
            _bag.Count.ShouldBe(3);
            _bag.ShouldBe([MonetaryValue.Create(200m, "USD"), Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void SetByCurrencyCode_NewCurrency_AddsValue()
        {
            _bag.SetAmount(75m, "AUD");
            _bag.Count.ShouldBe(4);
            _bag.ShouldBe([Cad50, Eur25, Usd100, Aud75], ignoreOrder: true);
        }

        [TestMethod]
        public void SetByCurrencyCode_ZeroAmount_ZeroesValue()
        {
            _bag.SetAmount(0, "USD");
            _bag.Count.ShouldBe(3);
            _bag.ShouldBe([Usd0, Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void SetByCurrencyCode_CurrencyDisallowed_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => _bag.SetAmount(123m, "XXXX"));
        }

        ////////////////////////////

        [TestMethod]

        public void SetByCurrency_CurrencyExists_UpdatesValue()
        {
            _bag.SetAmount(200m, Usd);
            _bag.Count.ShouldBe(3);
            _bag.ShouldBe([new(200m, "USD"), Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void SetByCurrency_CurrencyDoesNotExist_AddsValue()
        {
            _bag.SetAmount(75m, Aud);
            _bag.Count.ShouldBe(4);
            _bag.ShouldBe([Cad50, Eur25, Usd100, Aud75], ignoreOrder: true);
        }

        [TestMethod]
        public void SetByCurrency_ZeroAmount_ZeroesValue()
        {
            _bag.SetAmount(0, Usd);
            _bag.Count.ShouldBe(3);
            _bag.ShouldBe([Usd0, Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void SetByCurrency_CurrencyDisallowed_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => _bag.SetAmount(123m, Common.CurrencyX));
        }
    }
}