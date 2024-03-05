using PrefixClassName.MsTest;
using Shouldly;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[PrefixTestClass]
public class AddRangeTests
{
    private static readonly Money Usd100 = new(100m, "USD");
    private static readonly Money Cad50 = new(50m, "CAD");
    private static readonly Money Eur25 = new(25m, "EUR");
    private static readonly ImmutableSortedMoneySet ImmutableSet = [Usd100, Cad50, Eur25];

    private readonly SortedMoneySet _set = ImmutableSet.ToSet();

    [TestMethod]
    public void AllCurrenciesExist_UpdatesValues()
    {
        _set.AddRange([Usd100, Cad50, Eur25]);
        _set.Count.ShouldBe(3);
        _set.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")]);
    }

    [TestMethod]
    public void SomeNewCurrencies_UpdatesExistingAndAddsNewValues()
    {
        _set.AddRange([new(100m, "USD"), new(50m, "JPY"), new(25m, "CHF")]);
        _set.Count.ShouldBe(5);
        _set.ShouldBe([new(200m, "USD"), new(50m, "CAD"), new(25m, "EUR"), new(50m, "JPY"), new(25m, "CHF")]);
    }

    [TestMethod]
    public void AllNewCurrencies_AddsValues()
    {
        _set.AddRange([new(100m, "GBP"), new(50m, "JPY"), new(25m, "CHF")]);
        _set.Count.ShouldBe(6);
        _set.ShouldBe([new(100m, "USD"), new(50m, "CAD"), new(25m, "EUR"), new(100m, "GBP"), new(50m, "JPY"), new(25m, "CHF")]);
    }

    [TestMethod]
    public void EmptyCollection_NoChange()
    {
        _set.AddRange([]);
        _set.Count.ShouldBe(3);
        _set.ShouldBe(ImmutableSet);
    }

    [TestMethod]
    public void DisallowedCurrency_ThrowsArgumentException()
    {
        var disallowedCurrency = new Currency("Disallowed Currency", "XXX", "X", 2);
        IEnumerable<Money> values = [new(100m, "USD"), new(100m, disallowedCurrency), new(100m, disallowedCurrency), new(50m, "CAD"), new(25m, "EUR")];

        Should.Throw<ArgumentException>(() => _set.AddRange(values))
            .Message.ShouldBe($"The following currencies are not present in the set's currency registry: {disallowedCurrency} (Parameter 'values')");
        _set.Count.ShouldBe(3);
        _set.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")]);
    }

    [TestMethod]
    public void DisallowedCurrencies_ThrowsArgumentException()
    {
        var disallowedCurrencyX = new Currency("Disallowed Currency", "XXX", "X", 2);
        var disallowedCurrencyY = new Currency("Disallowed Currency2", "YYY", "Y", 2);
        IEnumerable<Money> values = [new(100m, "USD"), new(100m, disallowedCurrencyX), new(100m, disallowedCurrencyX), new(50m, "CAD"),
            new(100m, disallowedCurrencyY), new(100m, disallowedCurrencyY), new(25m, "EUR")];

        Should.Throw<ArgumentException>(() => _set.AddRange(values))
            .Message.ShouldBe($"The following currencies are not present in the set's currency registry: {disallowedCurrencyX}, {disallowedCurrencyY} (Parameter 'values')");
        _set.Count.ShouldBe(3);
        _set.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")]);
    }
}