namespace Singulink.Globalization.Tests.BagTests;

public static partial class Add
{
    [PrefixTestClass]
    public class TImmutableMoneyBag : Immutable<ImmutableMoneyBag>;

    [PrefixTestClass]
    public class TImmutableSortedMoneyBag : Immutable<ImmutableSortedMoneyBag>;

    public class Immutable<TBag> where TBag : IImmutableMoneyBag
    {
        private static readonly MonetaryValue Cad50 = new(50m, "CAD");
        private static readonly MonetaryValue Eur25 = new(25m, "EUR");
        private static readonly MonetaryValue Usd100 = new(100m, "USD");

        private static readonly IImmutableMoneyBag Bag = TBag.Create(CurrencyRegistry.Default, [Cad50, Usd100]);

        [TestMethod]
        public void AddMonetaryValue_CurrencyExists_UpdatesValue()
        {
            var resultBag = Bag.Add(Usd100);

            resultBag.Count.ShouldBe(2);
            resultBag.ShouldBe([Cad50, MonetaryValue.Create(200m, "USD")], ignoreOrder: !Bag.IsSorted);
        }

        [TestMethod]
        public void AddMonetaryValue_NewCurrency_AddsValue()
        {
            var resultBag = Bag.Add(Eur25);

            resultBag.Count.ShouldBe(3);
            resultBag.ShouldBe([Cad50, Eur25, Usd100], ignoreOrder: !Bag.IsSorted);
        }

        [TestMethod]
        public void AddMonetaryValue_DefaultValue_NoChange()
        {
            var resultBag = Bag.Add(default);

            resultBag.ShouldBeSameAs(Bag);
        }

        [TestMethod]
        public void AddMonetaryValue_CurrencyDisallowed_ThrowsException()
        {
            var value = new MonetaryValue(100, Common.CurrencyX);
            Should.Throw<ArgumentException>(() => Bag.Add(value));
        }

        ///////////////////////////

        [TestMethod]
        public void AddByCurrencyCode_CurrencyExists_UpdatesValue()
        {
            var resultBag = Bag.Add(100, "USD");
            resultBag.Count.ShouldBe(2);
            resultBag.ShouldBe([Cad50, new(200m, "USD")], ignoreOrder: !Bag.IsSorted);
        }

        [TestMethod]
        public void AddByCurrencyCode_NewCurrency_AddsValue()
        {
            var resultBag = Bag.Add(25m, "EUR");
            resultBag.Count.ShouldBe(3);
            resultBag.ShouldBe([Cad50, Eur25, Usd100], ignoreOrder: !Bag.IsSorted);
        }

        [TestMethod]
        public void AddByCurrencyCode_CurrencyDisallowed_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => Bag.Add(100m, Common.CurrencyX.CurrencyCode));
        }

        ///////////////////////////

        [TestMethod]
        public void AddByCurrency_CurrencyExists_UpdatesValue()
        {
            var resultBag = Bag.Add(100m, Currency.GetCurrency("USD"));
            resultBag.Count.ShouldBe(2);
            resultBag.ShouldBe([Cad50, new(200m, "USD")], ignoreOrder: !Bag.IsSorted);
        }

        [TestMethod]
        public void AddByCurrency_NewCurrency_AddsValue()
        {
            var resultBag = Bag.Add(25m, Currency.GetCurrency("EUR"));
            resultBag.Count.ShouldBe(3);
            resultBag.ShouldBe([Cad50, Eur25, Usd100], ignoreOrder: !Bag.IsSorted);
        }

        [TestMethod]
        public void AddByCurrency_CurrencyDisallowed_ThrowsArgumentException()
        {
            Bag.Count.ShouldBe(2);
            Should.Throw<ArgumentException>(() => Bag.Add(100m, Common.CurrencyX));
        }
    }
}