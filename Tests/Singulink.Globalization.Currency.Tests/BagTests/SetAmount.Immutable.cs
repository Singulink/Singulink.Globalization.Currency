namespace Singulink.Globalization.Tests.BagTests;

public static partial class SetAmount
{
    [PrefixTestClass]
    public class TImmutableMoneyBag : Immutable<ImmutableMoneyBag>;

    [PrefixTestClass]
    public class TImmutableSortedMoneyBag : Immutable<ImmutableSortedMoneyBag>;

    public class Immutable<TBag> where TBag : IImmutableMoneyBag
    {
        private static readonly Currency Usd = Currency.GetCurrency("USD");
        private static readonly Currency Aud = Currency.GetCurrency("AUD");

        private static readonly MonetaryValue Usd0 = new(0m, "USD");
        private static readonly MonetaryValue Usd100 = new(100m, "USD");
        private static readonly MonetaryValue Cad50 = new(50m, "CAD");
        private static readonly MonetaryValue Eur25 = new(25m, "EUR");
        private static readonly MonetaryValue Aud75 = new(75m, "AUD");

        private static readonly IImmutableMoneyBag Bag = TBag.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);

        [TestMethod]
        public void SetByCurrencyCode_CurrencyExists_UpdatesValue()
        {
            var resultBag = Bag.SetAmount(200m, "USD");
            resultBag.Count.ShouldBe(3);
            resultBag.ShouldBe([MonetaryValue.Create(200m, "USD"), Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void SetByCurrencyCode_NewCurrency_AddsValue()
        {
            var resultBag = Bag.SetAmount(75m, "AUD");
            resultBag.Count.ShouldBe(4);
            resultBag.ShouldBe([Cad50, Eur25, Usd100, Aud75], ignoreOrder: true);
        }

        [TestMethod]
        public void SetByCurrencyCode_ZeroAmount_ZeroesValue()
        {
            var resultBag = Bag.SetAmount(0, "USD");
            resultBag.Count.ShouldBe(3);
            resultBag.ShouldBe([Usd0, Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void SetByCurrencyCode_SetAlreadyContainsCurrencyCodeAndAmount_ReturnsItself()
        {
            var resultBag = Bag.SetAmount(100m, "USD");
            resultBag.ShouldBeSameAs(Bag);
        }

        [TestMethod]
        public void SetByCurrencyCode_CurrencyDisallowed_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => Bag.SetAmount(123m, "XXXX"));
        }

        /////////////////////////////////////////////

        [TestMethod]

        public void SetByCurrency_CurrencyExists_UpdatesValue()
        {
            var resultBag = Bag.SetAmount(200m, Usd);
            resultBag.Count.ShouldBe(3);
            resultBag.ShouldBe([new(200m, "USD"), Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void SetByCurrency_CurrencyDoesNotExist_AddsValue()
        {
            var resultBag = Bag.SetAmount(75m, Aud);
            resultBag.Count.ShouldBe(4);
            resultBag.ShouldBe([Cad50, Eur25, Usd100, Aud75], ignoreOrder: true);
        }

        [TestMethod]
        public void SetByCurrency_ZeroAmount_ZeroesValue()
        {
            var resultBag = Bag.SetAmount(0, Usd);
            resultBag.Count.ShouldBe(3);
            resultBag.ShouldBe([Usd0, Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void SetByCurrencyCode_BagAlreadyContainsCurrencyAndAmount_ReturnsItself()
        {
            var resultBag = Bag.SetAmount(100m, Usd);
            resultBag.ShouldBeSameAs(Bag);
        }

        [TestMethod]
        public void SetByCurrency_CurrencyDisallowed_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => Bag.SetAmount(123m, Common.CurrencyX));
        }
    }
}