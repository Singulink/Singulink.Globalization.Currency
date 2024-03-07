namespace Singulink.Globalization.Tests.MoneySetTests;

public static partial class RoundToCurrencyDigits
{
    public class Mutable<T> where T : IMoneySet
    {
#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
        private static readonly ImmutableArray<Money> RoundDownResults = [new(10.000m, "USD"), new(6.0m, "JPY")];
        private static readonly ImmutableArray<Money> RoundDownValues  = [new(10.004m, "USD"), new(6.2m, "JPY")];
        private static readonly ImmutableArray<Money> MidpointValues   = [new(10.005m, "USD"), new(6.5m, "JPY")];
        private static readonly ImmutableArray<Money> RoundUpValues    = [new(10.006m, "USD"), new(6.7m, "JPY")];
        private static readonly ImmutableArray<Money> RoundUpResults   = [new(10.010m, "USD"), new(7.0m, "JPY")];
#pragma warning restore SA1025

        private readonly IMoneySet _roundDownResults = T.Create(CurrencyRegistry.Default, RoundDownResults);
        private readonly IMoneySet _roundDownValues = T.Create(CurrencyRegistry.Default, RoundDownValues);
        private readonly IMoneySet _midpointValues = T.Create(CurrencyRegistry.Default, MidpointValues);
        private readonly IMoneySet _roundUpValues = T.Create(CurrencyRegistry.Default, RoundUpValues);
        private readonly IMoneySet _roundUpResults = T.Create(CurrencyRegistry.Default, RoundUpResults);

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