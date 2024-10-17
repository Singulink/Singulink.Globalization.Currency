namespace Singulink.Globalization.Tests.BagTests;

public static partial class Subtract
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

        private static readonly IImmutableMoneyBag Bag = TBag.Create(CurrencyRegistry.Default, [Usd100, Cad50]);

        [TestMethod]
        public void SubtractMonetaryValue_CurrencyExists_UpdatesValue()
        {
            var resultBag = Bag.Subtract(-Usd100);
            resultBag.Count.ShouldBe(2);
            resultBag.ShouldBe([Cad50, MonetaryValue.Create(200m, "USD")], ignoreOrder: true);
        }

        [TestMethod]
        public void SubtractMonetaryValue_NewCurrency_AddsValue()
        {
            var resultBag = Bag.Subtract(-Eur25);
            resultBag.Count.ShouldBe(3);
            resultBag.ShouldBe([Cad50, Eur25, Usd100], ignoreOrder: true);
        }

        [TestMethod]
        public void SubtractMonetaryValue_DefaultValue_NoChange()
        {
            var resultBag = Bag.Subtract(default);
            resultBag.Count.ShouldBe(2);
            resultBag.ShouldBe(Bag, ignoreOrder: true);
        }

        [TestMethod]
        public void SubtractMonetaryValue_CurrencyDisallowed_ThrowsArgumentException()
        {
            var value = new MonetaryValue(100, Common.DisallowedCurrency);
            Should.Throw<ArgumentException>(() => Bag.Subtract(value));
        }

        /////////////////////////

        [TestMethod]
        public void SubtractByCurrencyCode_CurrencyExists_UpdatesValue()
        {
            var resultBag = Bag.Subtract(-100, "USD");
            resultBag.Count.ShouldBe(2);
            resultBag.ShouldBe([Cad50, new(200m, "USD")], ignoreOrder: true);
        }

        [TestMethod]
        public void SubtractByCurrencyCode_NewCurrency_AddsValue()
        {
            var resultBag = Bag.Subtract(-25m, "EUR");
            resultBag.Count.ShouldBe(3);
            resultBag.ShouldBe([Cad50, Eur25, Usd100], ignoreOrder: true);
        }

        [TestMethod]
        public void SubtractByCurrencyCode_CurrencyDisallowed_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => Bag.Subtract(100m, Common.DisallowedCurrency.CurrencyCode));
        }

        /////////////////////////

        [TestMethod]
        public void SubtractByCurrency_CurrencyExists_UpdatesValue()
        {
            var resultBag = Bag.Subtract(-100m, Currency.GetCurrency("USD"));
            resultBag.Count.ShouldBe(2);
            resultBag.ShouldBe([Cad50, new(200m, "USD")], ignoreOrder: true);
        }

        [TestMethod]
        public void SubtractByCurrency_NewCurrency_AddsValue()
        {
            var resultBag = Bag.Subtract(-25m, Currency.GetCurrency("EUR"));
            resultBag.Count.ShouldBe(3);
            resultBag.ShouldBe([Cad50, Eur25, Usd100], ignoreOrder: true);
        }

        [TestMethod]
        public void SubtractByCurrency_CurrencyDisallowed_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => Bag.Subtract(100m, Common.DisallowedCurrency));
        }
    }
}