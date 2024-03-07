﻿namespace Singulink.Globalization.Tests.ImmutableSortedMoneySetTests;

[PrefixTestClass]
public class AddRange
{
    private static readonly Money Usd100 = new(100m, "USD");
    private static readonly Money Cad50 = new(50m, "CAD");
    private static readonly Money Eur25 = new(25m, "EUR");
    private static readonly ImmutableSortedMoneySet Set = [Usd100, Cad50, Eur25];

    [TestMethod]
    public void AllCurrenciesExist_UpdatesValues()
    {
        var resultSet = Set.AddRange([Usd100, Cad50, Eur25]);
        resultSet.Count.ShouldBe(3);
        resultSet.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")]);
    }

    [TestMethod]
    public void SomeNewCurrencies_UpdatesExistingAndAddsNewValues()
    {
        var resultSet = Set.AddRange([new(100m, "USD"), new(50m, "JPY"), new(25m, "CHF"), new(0m, "CAD")]);
        resultSet.Count.ShouldBe(5);
        resultSet.ShouldBe([new(200m, "USD"), new(50m, "CAD"), new(25m, "EUR"), new(50m, "JPY"), new(25m, "CHF")]);
    }

    [TestMethod]
    public void AllNewCurrencies_AddsValues()
    {
        var resultSet = Set.AddRange([new(100m, "GBP"), new(50m, "JPY"), new(25m, "CHF"), default]);
        resultSet.Count.ShouldBe(6);
        resultSet.ShouldBe([new(100m, "USD"), new(50m, "CAD"), new(25m, "EUR"), new(100m, "GBP"), new(50m, "JPY"), new(25m, "CHF")]);
    }

    [TestMethod]
    public void EmptyCollection_NoChange()
    {
        var resultSet = Set.AddRange([]);
        resultSet.Count.ShouldBe(3);
        resultSet.ShouldBe(Set);
    }

    [TestMethod]
    public void DisallowedCurrency_ThrowsArgumentException()
    {
        var disallowedCurrency = new Currency("Disallowed Currency", "XXX", "X", 2);
        IEnumerable<Money> values = [new(100m, "USD"), new(100m, disallowedCurrency), new(50m, "CAD"), new(25m, "EUR")];

        Should.Throw<ArgumentException>(() => Set.AddRange(values));
    }
}