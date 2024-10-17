namespace Singulink.Globalization.Tests.BagTests;

public static partial class TransformValues
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
        private static readonly IImmutableMoneyBag Bag = TBag.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);

        [TestMethod]
        public void NonNullOutput_AllAmountsTransformed_UpdatesValues()
        {
            var resultBag = Bag.TransformValues(x => x.Amount * 2);
            resultBag.Count.ShouldBe(3);
            resultBag.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")], ignoreOrder: true);
        }

        [TestMethod]
        public void NonNullOutput_IdentityTransform_NoChange()
        {
            var resultBag = Bag.TransformValues(x => x.Amount);
            resultBag.ShouldBeSameAs(Bag);
        }

        [TestMethod]
        public void NonNullOutput_EmptyBag_NoChange()
        {
            IImmutableMoneyBag emptyBag = TBag.Create(CurrencyRegistry.Default, []);
            var resultBag = emptyBag.TransformValues(x => x.Amount * 2);
            resultBag.ShouldBeSameAs(emptyBag);
        }

        ///////////////////////////

        [TestMethod]
        public void NullableOutput_AllAmountsTransformed_UpdatesValues()
        {
            var resultBag = Bag.TransformValues(x => (decimal?)x.Amount * 2);
            resultBag.Count.ShouldBe(3);
            resultBag.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")], ignoreOrder: true);
        }

        [TestMethod]
        public void NullableOutput_IdentityTransform_NoChange()
        {
            var resultBag = Bag.TransformValues(x => (decimal?)x.Amount);
            resultBag.ShouldBeSameAs(Bag);
        }

        [TestMethod]
        public void NullableOutput_EmptyBag_NoChange()
        {
            IImmutableMoneyBag emptyBag = TBag.Create(CurrencyRegistry.Default, []);
            var resultBag = emptyBag.TransformValues(x => (decimal?)x.Amount * 2);
            resultBag.ShouldBeSameAs(emptyBag);
        }

        [TestMethod]
        public void NullableOutput_NullTransform_RemovesNullValues()
        {
            var resultBag = Bag.TransformValues(x => x.Amount == 100m ? null : x.Amount);
            resultBag.Count.ShouldBe(2);
            resultBag.ShouldBe([new(50m, "CAD"), new(25m, "EUR")], ignoreOrder: true);
        }
    }
}