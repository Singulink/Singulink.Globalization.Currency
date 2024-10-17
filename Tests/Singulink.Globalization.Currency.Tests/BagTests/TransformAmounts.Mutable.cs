namespace Singulink.Globalization.Tests.BagTests;

public static partial class TransformAmounts
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
        private static readonly ImmutableArray<MonetaryValue> DefaultBagValues = [Usd100, Cad50, Eur25];

        private readonly IMoneyBag _bag = TBag.Create(CurrencyRegistry.Default, DefaultBagValues);

        [TestMethod]
        public void NonNullOutput_AllAmountsTransformed_UpdatesValue()
        {
            _bag.TransformAmounts(x => x * 2);
            _bag.Count.ShouldBe(3);
            _bag.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")], ignoreOrder: true);
        }

        [TestMethod]
        public void NonNullOutput_IdentityTransform_NoChange()
        {
            _bag.TransformAmounts(x => x);
            _bag.ShouldBe(DefaultBagValues, ignoreOrder: true);
        }

        [TestMethod]
        public void NonNullOutput_EmptyBag_NoChange()
        {
            IMoneyBag emptyBag = TBag.Create(CurrencyRegistry.Default, []);
            emptyBag.TransformAmounts(x => x * 2);
            emptyBag.Count.ShouldBe(0);
        }

        ///////////////////////////

        [TestMethod]
        public void NullableOutput_AllAmountsTransformed_UpdatesValue()
        {
            _bag.TransformAmounts(x => (decimal?)x * 2);
            _bag.Count.ShouldBe(3);
            _bag.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")], ignoreOrder: true);
        }

        [TestMethod]
        public void NullableOutput_IdentityTransform_NoChange()
        {
            _bag.TransformAmounts(x => (decimal?)x);
            _bag.ShouldBe(DefaultBagValues, ignoreOrder: true);
        }

        [TestMethod]
        public void NullableOutput_EmptyBag_NoChange()
        {
            IMoneyBag emptyBag = TBag.Create(CurrencyRegistry.Default, []);
            emptyBag.TransformAmounts(x => (decimal?)x * 2);
            emptyBag.Count.ShouldBe(0);
        }

        [TestMethod]
        public void NullableOutput_NullTransform_RemovesNullValues()
        {
            _bag.TransformAmounts(x => x == 100m ? null : x);
            _bag.Count.ShouldBe(2);
            _bag.ShouldBe([new(50m, "CAD"), new(25m, "EUR")], ignoreOrder: true);
        }
    }
}