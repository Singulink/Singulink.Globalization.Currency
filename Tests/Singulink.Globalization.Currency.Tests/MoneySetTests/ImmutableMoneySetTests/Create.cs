﻿namespace Singulink.Globalization.Tests.MoneySetTests.ImmutableMoneySetTests;

[PrefixTestClass]
public class Create
{
    private static readonly Money Usd100 = new Money(100m, "USD");
    private static readonly ImmutableArray<Money> SetValues = [Usd100, new(50m, "CAD"), new(25m, "EUR")];

    [TestMethod]
    public void Create_SingleValue()
    {
        var set = ImmutableMoneySet.Create(Usd100);
        set.Count.ShouldBe(1);
        set.ShouldBe([Usd100]);
    }

    [TestMethod]
    public void Create_EmptyArray()
    {
        var set = ImmutableMoneySet.Create(Array.Empty<Money>());
        set.Count.ShouldBe(0);
        set.ShouldBe([]);
    }

    [TestMethod]
    public void Create_SomeDefaultValues_IgnoresDefaultValues()
    {
        var set = ImmutableMoneySet.Create([default, ..SetValues, default]);
        set.Count.ShouldBe(3);
        set.ShouldBe(SetValues, ignoreOrder: true);
    }

    [TestMethod]
    public void Create_ZeroValues_SkipsIfPresent()
    {
        var set = ImmutableMoneySet.Create([..SetValues, new(0m, "USD")]);
        set.Count.ShouldBe(3);
        set.ShouldBe(SetValues, ignoreOrder: true);
    }

    [TestMethod]
    public void Create_MultipleSameCurrencyValues_AddsSameCurrencyValuesTogether()
    {
        var set = ImmutableMoneySet.Create([..SetValues, ..SetValues, ..SetValues]);
        set.Count.ShouldBe(3);
        set.ShouldBe([new(300m, "USD"), new(150m, "CAD"), new(75m, "EUR")], ignoreOrder: true);
    }
}