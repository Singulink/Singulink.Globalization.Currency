namespace Singulink.Globalization.Tests.CurrencyRegistryTests;

[PrefixTestClass]
public class IsReadOnly
{
    [TestMethod]
    public void IsReadOnlyCollection()
    {
        CurrencyRegistry.Default.ShouldBeReadOnlyCollection();
    }
}
