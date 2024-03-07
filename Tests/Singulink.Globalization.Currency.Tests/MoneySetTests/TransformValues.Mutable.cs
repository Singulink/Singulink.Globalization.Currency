namespace Singulink.Globalization.Tests.MoneySetTests;

public static partial class TransformValues
{
    public class Mutable<T> where T : IMoneySet
    {
        private static readonly Money Usd100 = new(100m, "USD");
        private static readonly Money Cad50 = new(50m, "CAD");
        private static readonly Money Eur25 = new(25m, "EUR");
        private static readonly ImmutableArray<Money> DefaultSetValues = [Usd100, Cad50, Eur25];

        private readonly IMoneySet _set = T.Create(CurrencyRegistry.Default, DefaultSetValues);

        [TestMethod]
        public void NonNullOutput_AllAmountsTransformed_UpdatesValue()
        {
            _set.TransformValues(x => x.Amount * 2);
            _set.Count.ShouldBe(3);
            _set.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")], ignoreOrder: true);
        }

        [TestMethod]
        public void NonNullOutput_IdentityTransform_NoChange()
        {
            _set.TransformValues(x => x.Amount);
            _set.ShouldBe(DefaultSetValues, ignoreOrder: true);
        }

        [TestMethod]
        public void NonNullOutput_EmptySet_NoChange()
        {
            SortedMoneySet emptySet = [];
            emptySet.TransformValues(x => x.Amount * 2);
            emptySet.Count.ShouldBe(0);
        }

        [TestMethod]
        public void NullableOutput_AllAmountsTransformed_UpdatesValue()
        {
            _set.TransformValues(x => (decimal?)x.Amount * 2);
            _set.Count.ShouldBe(3);
            _set.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")], ignoreOrder: true);
        }

        [TestMethod]
        public void NullableOutput_IdentityTransform_NoChange()
        {
            _set.TransformValues(x => (decimal?)x.Amount);
            _set.ShouldBe(DefaultSetValues, ignoreOrder: true);
        }

        [TestMethod]
        public void NullableOutput_EmptySet_NoChange()
        {
            SortedMoneySet emptySet = [];
            emptySet.TransformValues(x => (decimal?)x.Amount * 2);
            emptySet.Count.ShouldBe(0);
        }

        [TestMethod]
        public void NullableOutput_NullTransform_RemovesNullValues()
        {
            _set.TransformValues(x => x.Amount == 100m ? null : x.Amount);
            _set.Count.ShouldBe(2);
            _set.ShouldBe([new(50m, "CAD"), new(25m, "EUR")], ignoreOrder: true);
        }
    }
}