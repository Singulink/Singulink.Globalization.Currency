using System.Globalization;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[PrefixTestClass]
public class ToString
{
    private const char Sp = '\u00A0'; // The space char that ToString uses (non-breaking space)

    private static readonly CultureInfo EnUS = CultureInfo.GetCultureInfo("en-US");
    private static readonly CultureInfo FrFR = CultureInfo.GetCultureInfo("fe-FR");
    private static readonly ImmutableSortedMoneySet Set = [Money.Create(1.23m, "USD"), Money.Create(0m, "CAD"), Money.Create(500m, "JPY")];

    private readonly SortedMoneySet _set = Set.ToSortedMoneySet();

    [TestMethod]
    public void DefaultFormat()
    {
        CultureInfo.CurrentCulture = EnUS;

        _set.ToString().ShouldBe($"CAD{Sp}0.00, JPY{Sp}500, USD{Sp}1.23");
        _set.ToString(null, FrFR).ShouldBe($"0.00{Sp}CAD, 500{Sp}JPY, 1.23{Sp}USD");
    }

    [TestMethod]
    public void ZeroAmountsIgnoredFormat()
    {
        CultureInfo.CurrentCulture = EnUS;

        _set.ToString("!").ShouldBe($"JPY{Sp}500, USD{Sp}1.23");
        _set.ToString("!", FrFR).ShouldBe($"500{Sp}JPY, 1.23{Sp}USD");
    }

    [TestMethod]
    public void CustomMoneyFormat()
    {
        CultureInfo.CurrentCulture = EnUS;

        _set.ToString("SN$0").ShouldBe("$0, ￥500, $1");
    }

    [TestMethod]
    public void CustomMoneyWithZeroAmountsIgnoredFormat()
    {
        CultureInfo.CurrentCulture = EnUS;

        _set.ToString("!SN$0").ShouldBe("￥500, $1");
    }
}
