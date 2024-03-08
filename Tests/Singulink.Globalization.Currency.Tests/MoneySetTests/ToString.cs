using System.Globalization;

namespace Singulink.Globalization.Tests.MoneySetTests;

public static class ToString
{
    [PrefixTestClass]
    public class TMoneySet : Tests<MoneySet>;

    [PrefixTestClass]
    public class TSortedMoneySet : Tests<SortedMoneySet>;

    [PrefixTestClass]
    public class TImmutableMoneySet : Tests<ImmutableMoneySet>;

    [PrefixTestClass]
    public class TImmutableSortedMoneySet : Tests<ImmutableSortedMoneySet>;

    public class Tests<T> where T : IReadOnlyMoneySet
    {
        private const char Sp = '\u00A0'; // The space char that ToString uses (non-breaking space)

        private static readonly CultureInfo EnUS = CultureInfo.GetCultureInfo("en-US");
        private static readonly CultureInfo FrFR = CultureInfo.GetCultureInfo("fe-FR");

        private readonly IReadOnlyMoneySet _set = T.Create(CurrencyRegistry.Default, [Money.Create(1.23m, "USD"), Money.Create(0m, "CAD"), Money.Create(500m, "JPY")]);

        [TestMethod]
        public void DefaultFormat()
        {
            CultureInfo.CurrentCulture = EnUS;

            ListStringShouldBe(_set.ToString(), $"CAD{Sp}0.00, JPY{Sp}500, USD{Sp}1.23");
            ListStringShouldBe(_set.ToString(null, FrFR), $"0.00{Sp}CAD, 500{Sp}JPY, 1.23{Sp}USD");
        }

        [TestMethod]
        public void ZeroAmountsIgnoredFormat()
        {
            CultureInfo.CurrentCulture = EnUS;

            ListStringShouldBe(_set.ToString("!", null), $"JPY{Sp}500, USD{Sp}1.23");
            ListStringShouldBe(_set.ToString("!", FrFR), $"500{Sp}JPY, 1.23{Sp}USD");
        }

        [TestMethod]
        public void CustomMoneyFormat()
        {
            ListStringShouldBe(_set.ToString("SN$0", EnUS), "$0, ￥500, $1");
        }

        [TestMethod]
        public void CustomMoneyWithZeroAmountsIgnoredFormat()
        {
            ListStringShouldBe(_set.ToString("!SN$0", EnUS), "￥500, $1");
        }

        private void ListStringShouldBe(string? values1, string values2)
        {
            if (_set.IsSorted)
                values1.ShouldBe(values2);
            else
                values1?.Split(", ").ShouldBe(values2.Split(", "), ignoreOrder: true);
        }
    }
}