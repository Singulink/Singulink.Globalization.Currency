using Shouldly;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[TestClass]
public class SetValueTests
{
    private static readonly Money _usd100 = new(100m, "USD");
    private static readonly Money _cad50 = new(50m, "CAD");
    private static readonly Money _eur25 = new(25m, "EUR");
    private static readonly Money _aud75 = new(75m, "AUD");
    private static readonly ImmutableSortedMoneySet _immutableSet = [_usd100, _cad50, _eur25];
    private readonly SortedMoneySet _set = _immutableSet.ToSet();

    [TestMethod]
    public void SetValue_CurrencyDoesNotExist_AddsValue()
    {
        _set.SetValue(_aud75);
        _set.Count.ShouldBe(4);
        _set.ShouldBe([_cad50, _eur25, _usd100, _aud75]);
    }

    [TestMethod]
    public void SetValue_CurrencyExists_UpdatesValue()
    {
        _set.SetValue(new(200m, "USD"));
        _set.Count.ShouldBe(3);
        _set.ShouldBe([new(200m, "USD"), _cad50, _eur25]);
    }

    [TestMethod]
    public void SetValue_DefaultValue_NoChange()
    {
        _set.SetValue(default);
        _set.Count.ShouldBe(3);
        _set.ShouldBe(_immutableSet);
    }

    [TestMethod]
    public void SetValue_CurrencyIsNotAccepted_ThrowsArgumentException()
    {
        var value = new Money(100, new Currency("Blah blah blah", "BBB", "$$", 2));
        Should.Throw<ArgumentException>(() => _set.SetValue(value));
    }
}