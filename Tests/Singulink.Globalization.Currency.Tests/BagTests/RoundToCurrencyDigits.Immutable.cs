namespace Singulink.Globalization.Tests.BagTests;

public static partial class RoundToCurrencyDigits
{
    [PrefixTestClass]
    public class TImmutableMoneyBag : Immutable<ImmutableMoneyBag>;

    [PrefixTestClass]
    public class TImmutableSortedMoneyBag : Immutable<ImmutableSortedMoneyBag>;

    public class Immutable<TBag> where TBag : IImmutableMoneyBag
    {
#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
        private static readonly IImmutableMoneyBag RoundDownResults = TBag.Create(CurrencyRegistry.Default, [new(10.000m, "USD"), new(6.0m, "JPY")]);
        private static readonly IImmutableMoneyBag RoundDownValues  = TBag.Create(CurrencyRegistry.Default, [new(10.004m, "USD"), new(6.2m, "JPY")]);
        private static readonly IImmutableMoneyBag MidpointValues   = TBag.Create(CurrencyRegistry.Default, [new(10.005m, "USD"), new(6.5m, "JPY")]);
        private static readonly IImmutableMoneyBag RoundUpValues    = TBag.Create(CurrencyRegistry.Default, [new(10.006m, "USD"), new(6.7m, "JPY")]);
        private static readonly IImmutableMoneyBag RoundUpResults   = TBag.Create(CurrencyRegistry.Default, [new(10.010m, "USD"), new(7.0m, "JPY")]);
#pragma warning restore SA1025

        [TestMethod]
        public void DefaultToEven()
        {
            RoundDownResults.RoundToCurrencyDigits().ShouldBeSameAs(RoundDownResults);
            RoundDownValues.RoundToCurrencyDigits().ShouldBe(RoundDownResults, ignoreOrder: true);
            MidpointValues.RoundToCurrencyDigits().ShouldBe(RoundDownResults, ignoreOrder: true);
            RoundUpValues.RoundToCurrencyDigits().ShouldBe(RoundUpResults, ignoreOrder: true);
            RoundUpResults.RoundToCurrencyDigits().ShouldBeSameAs(RoundUpResults);
        }

        [TestMethod]
        public void AwayFromZero()
        {
            const MidpointRounding mode = MidpointRounding.AwayFromZero;

            RoundDownResults.RoundToCurrencyDigits(mode).ShouldBeSameAs(RoundDownResults);
            RoundDownValues.RoundToCurrencyDigits(mode).ShouldBe(RoundDownResults, ignoreOrder: true);
            MidpointValues.RoundToCurrencyDigits(mode).ShouldBe(RoundUpResults, ignoreOrder: true);
            RoundUpValues.RoundToCurrencyDigits(mode).ShouldBe(RoundUpResults, ignoreOrder: true);
            RoundUpResults.RoundToCurrencyDigits(mode).ShouldBeSameAs(RoundUpResults);
        }
    }
}