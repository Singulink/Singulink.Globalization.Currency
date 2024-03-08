namespace Singulink.Globalization.Tests.Sets;

public static partial class TransformValues
{
    [PrefixTestClass]
    public class TImmutableMoneySet : Immutable<ImmutableMoneySet>;

    [PrefixTestClass]
    public class TImmutableSortedMoneySet : Immutable<ImmutableSortedMoneySet>;

    public class Immutable<T> where T : IImmutableMoneySet
    {
        private static readonly Money Usd100 = new(100m, "USD");
        private static readonly Money Cad50 = new(50m, "CAD");
        private static readonly Money Eur25 = new(25m, "EUR");
        private static readonly IImmutableMoneySet Set = T.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);

        [TestMethod]
        public void NonNullOutput_AllAmountsTransformed_UpdatesValues()
        {
            var resultSet = Set.TransformValues(x => x.Amount * 2);
            resultSet.Count.ShouldBe(3);
            resultSet.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")], ignoreOrder: true);
        }

        [TestMethod]
        public void NonNullOutput_IdentityTransform_NoChange()
        {
            var resultSet = Set.TransformValues(x => x.Amount);
            resultSet.ShouldBeSameAs(Set);
        }

        [TestMethod]
        public void NonNullOutput_EmptySet_NoChange()
        {
            IImmutableMoneySet emptySet = T.Create(CurrencyRegistry.Default, []);
            var resultSet = emptySet.TransformValues(x => x.Amount * 2);
            resultSet.ShouldBeSameAs(emptySet);
        }

        ///////////////////////////

        [TestMethod]
        public void NullableOutput_AllAmountsTransformed_UpdatesValues()
        {
            var resultSet = Set.TransformValues(x => (decimal?)x.Amount * 2);
            resultSet.Count.ShouldBe(3);
            resultSet.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")], ignoreOrder: true);
        }

        [TestMethod]
        public void NullableOutput_IdentityTransform_NoChange()
        {
            var resultSet = Set.TransformValues(x => (decimal?)x.Amount);
            resultSet.ShouldBeSameAs(Set);
        }

        [TestMethod]
        public void NullableOutput_EmptySet_NoChange()
        {
            IImmutableMoneySet emptySet = T.Create(CurrencyRegistry.Default, []);
            var resultSet = emptySet.TransformValues(x => (decimal?)x.Amount * 2);
            resultSet.ShouldBeSameAs(emptySet);
        }

        [TestMethod]
        public void NullableOutput_NullTransform_RemovesNullValues()
        {
            var resultSet = Set.TransformValues(x => x.Amount == 100m ? null : x.Amount);
            resultSet.Count.ShouldBe(2);
            resultSet.ShouldBe([new(50m, "CAD"), new(25m, "EUR")], ignoreOrder: true);
        }
    }
}