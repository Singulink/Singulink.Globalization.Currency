namespace Singulink.Globalization.Tests.BagTests;

[PrefixTestClass]
public class InterfaceTypeArguments
{
    // The bag interfaces have no static abstract members, so they can be used as type arguments.

    [TestMethod]
    public async Task InterfacesAsTypeArguments()
    {
        List<IMoneyBag> bags = [new MoneyBag(new MonetaryValue(1m, "USD")), new SortedMoneyBag(new MonetaryValue(2m, "EUR"))];
        bags.Count.ShouldBe(2);

        var accounts = new Dictionary<string, IReadOnlyMoneyBag> { ["cash"] = bags[0], ["frozen"] = ImmutableMoneyBag.Create(new MonetaryValue(3m, "CAD")) };
        accounts["frozen"]["CAD"].ShouldBe(new MonetaryValue(3m, "CAD"));

        IImmutableMoneyBag immutable = await Task.FromResult<IImmutableMoneyBag>(ImmutableSortedMoneyBag.Create(new MonetaryValue(4m, "USD")));
        immutable["USD"].Amount.ShouldBe(4m);

        Func<IReadOnlyMoneyBag> factory = () => bags[1];
        factory()["EUR"].Amount.ShouldBe(2m);
    }
}
