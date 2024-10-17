using System.Globalization;

namespace Singulink.Globalization.Tests.BagTests;

public static class ToString
{
    [PrefixTestClass]
    public class TMoneyBag : Tests<MoneyBag>;

    [PrefixTestClass]
    public class TSortedMoneyBag : Tests<SortedMoneyBag>;

    [PrefixTestClass]
    public class TImmutableMoneyBag : Tests<ImmutableMoneyBag>;

    [PrefixTestClass]
    public class TImmutableSortedMoneyBag : Tests<ImmutableSortedMoneyBag>;

    public class Tests<TBag> where TBag : IReadOnlyMoneyBag
    {
        private const char Sp = '\u00A0'; // The space char that ToString uses (non-breaking space)

        private static readonly CultureInfo EnUS = CultureInfo.GetCultureInfo("en-US");
        private static readonly CultureInfo FrFR = CultureInfo.GetCultureInfo("fe-FR");

        private readonly IReadOnlyMoneyBag _bag = TBag.Create(CurrencyRegistry.Default, [MonetaryValue.Create(1.23m, "USD"), MonetaryValue.Create(0m, "CAD"), MonetaryValue.Create(500m, "JPY")]);

        [TestMethod]
        public void DefaultFormat()
        {
            CultureInfo.CurrentCulture = EnUS;

            ListStringShouldBe(_bag.ToString(), $"CAD{Sp}0.00, JPY{Sp}500, USD{Sp}1.23");
            ListStringShouldBe(_bag.ToString(null, FrFR), $"0.00{Sp}CAD, 500{Sp}JPY, 1.23{Sp}USD");
        }

        [TestMethod]
        public void ZeroAmountsIgnoredFormat()
        {
            CultureInfo.CurrentCulture = EnUS;

            ListStringShouldBe(_bag.ToString("!", null), $"JPY{Sp}500, USD{Sp}1.23");
            ListStringShouldBe(_bag.ToString("!", FrFR), $"500{Sp}JPY, 1.23{Sp}USD");
        }

        [TestMethod]
        public void CustomMonetaryFormat()
        {
            ListStringShouldBe(_bag.ToString("CNB0", EnUS), "$0, ￥500, $1");
        }

        [TestMethod]
        public void CustomMonetaryWithZeroAmountsIgnoredFormat()
        {
            ListStringShouldBe(_bag.ToString("!CNB0", EnUS), "￥500, $1");
        }

        private void ListStringShouldBe(string? values1, string values2)
        {
            if (_bag.IsSorted)
                values1.ShouldBe(values2);
            else
                values1?.Split(", ").ShouldBe(values2.Split(", "), ignoreOrder: true);
        }
    }
}