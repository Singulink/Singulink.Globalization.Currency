namespace Singulink.Globalization.Tests.BagTests;

public static partial class SetValue
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
        private static readonly MonetaryValue Aud75 = new(75m, "AUD");
        private static readonly IImmutableMoneyBag Bag = TBag.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);

        [TestMethod]
        public void SetValue_CurrencyDoesNotExist_AddsValue()
        {
            var resultBag = Bag.SetValue(Aud75);
            resultBag.Count.ShouldBe(4);
            resultBag.ShouldBe([Cad50, Eur25, Usd100, Aud75], ignoreOrder: true);
        }

        [TestMethod]
        public void SetValue_CurrencyExists_UpdatesValue()
        {
            var resultBag = Bag.SetValue(new(200m, "USD"));
            resultBag.Count.ShouldBe(3);
            resultBag.ShouldBe([new(200m, "USD"), Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void SetValue_DefaultValue_NoChange()
        {
            var resultBag = Bag.SetValue(default);
            resultBag.ShouldBeSameAs(Bag);
        }

        [TestMethod]
        public void SetValue_ValueAlreadyExists_NoChange()
        {
            var resultBag = Bag.SetAmount(100m, "USD");
            resultBag.ShouldBeSameAs(Bag);
        }

        [TestMethod]
        public void SetValue_CurrencyDisallowed_ThrowsArgumentException()
        {
            var value = new MonetaryValue(100, Common.CurrencyX);
            Should.Throw<ArgumentException>(() => Bag.SetValue(value));
        }
    }
}