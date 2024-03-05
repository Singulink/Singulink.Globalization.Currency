using PrefixClassName.MsTest;
using Shouldly;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[PrefixTestClass]
public class SetValueTests
{
    private static readonly Money Usd100 = new(100m, "USD");
    private static readonly Money Cad50 = new(50m, "CAD");
    private static readonly Money Eur25 = new(25m, "EUR");
    private static readonly Money Aud75 = new(75m, "AUD");
    private static readonly ImmutableSortedMoneySet ImmutableSet = [Usd100, Cad50, Eur25];

    private readonly SortedMoneySet _set = ImmutableSet.ToSet();

    [TestMethod]
    public void SetValue_CurrencyDoesNotExist_AddsValue()
    {
        _set.SetValue(Aud75);
        _set.Count.ShouldBe(4);
        _set.ShouldBe([Cad50, Eur25, Usd100, Aud75]);
    }

    [TestMethod]
    public void SetValue_CurrencyExists_UpdatesValue()
    {
        _set.SetValue(new(200m, "USD"));
        _set.Count.ShouldBe(3);
        _set.ShouldBe([new(200m, "USD"), Cad50, Eur25]);
    }

    [TestMethod]
    public void SetValue_DefaultValue_NoChange()
    {
        _set.SetValue(default);
        _set.Count.ShouldBe(3);
        _set.ShouldBe(ImmutableSet);
    }

    [TestMethod]
    public void SetValue_CurrencyDisallowed_ThrowsArgumentException()
    {
        var value = new Money(100, new Currency("Blah blah blah", "BBB", "$$", 2));
        Should.Throw<ArgumentException>(() => _set.SetValue(value));
    }
}