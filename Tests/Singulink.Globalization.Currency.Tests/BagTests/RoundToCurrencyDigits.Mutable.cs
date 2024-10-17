namespace Singulink.Globalization.Tests.BagTests;

public static partial class RoundToCurrencyDigits
{
    [PrefixTestClass]
    public class TMoneyBag : Mutable<MoneyBag>;

    [PrefixTestClass]
    public class TSortedMoneyBag : Mutable<SortedMoneyBag>;

    public class Mutable<TBag> where TBag : IMoneyBag
    {
#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
        private static readonly ImmutableArray<MonetaryValue> RoundDownResults = [new(10.000m, "USD"), new(6.0m, "JPY")];
        private static readonly ImmutableArray<MonetaryValue> RoundDownValues  = [new(10.004m, "USD"), new(6.2m, "JPY")];
        private static readonly ImmutableArray<MonetaryValue> MidpointValues   = [new(10.005m, "USD"), new(6.5m, "JPY")];
        private static readonly ImmutableArray<MonetaryValue> RoundUpValues    = [new(10.006m, "USD"), new(6.7m, "JPY")];
        private static readonly ImmutableArray<MonetaryValue> RoundUpResults   = [new(10.010m, "USD"), new(7.0m, "JPY")];
#pragma warning restore SA1025

        private readonly IMoneyBag _roundDownResults = TBag.Create(CurrencyRegistry.Default, RoundDownResults);
        private readonly IMoneyBag _roundDownValues = TBag.Create(CurrencyRegistry.Default, RoundDownValues);
        private readonly IMoneyBag _midpointValues = TBag.Create(CurrencyRegistry.Default, MidpointValues);
        private readonly IMoneyBag _roundUpValues = TBag.Create(CurrencyRegistry.Default, RoundUpValues);
        private readonly IMoneyBag _roundUpResults = TBag.Create(CurrencyRegistry.Default, RoundUpResults);

        [TestMethod]
        public void DefaultToEven()
        {
            _roundDownResults.RoundToCurrencyDigits();
            _roundDownValues.RoundToCurrencyDigits();
            _midpointValues.RoundToCurrencyDigits();
            _roundUpValues.RoundToCurrencyDigits();
            _roundUpResults.RoundToCurrencyDigits();

            _roundDownResults.ShouldBe(RoundDownResults, ignoreOrder: true);
            _roundDownValues.ShouldBe(RoundDownResults, ignoreOrder: true);
            _midpointValues.ShouldBe(RoundDownResults, ignoreOrder: true);
            _roundUpValues.ShouldBe(RoundUpResults, ignoreOrder: true);
            _roundUpResults.ShouldBe(RoundUpResults, ignoreOrder: true);
        }

        [TestMethod]
        public void AwayFromZero()
        {
            const MidpointRounding mode = MidpointRounding.AwayFromZero;

            _roundDownResults.RoundToCurrencyDigits(mode);
            _roundDownValues.RoundToCurrencyDigits(mode);
            _midpointValues.RoundToCurrencyDigits(mode);
            _roundUpValues.RoundToCurrencyDigits(mode);
            _roundUpResults.RoundToCurrencyDigits(mode);

            _roundDownResults.ShouldBe(RoundDownResults, ignoreOrder: true);
            _roundDownValues.ShouldBe(RoundDownResults, ignoreOrder: true);
            _midpointValues.ShouldBe(RoundUpResults, ignoreOrder: true);
            _roundUpValues.ShouldBe(RoundUpResults, ignoreOrder: true);
            _roundUpResults.ShouldBe(RoundUpResults, ignoreOrder: true);
        }
    }
}