namespace Singulink.Globalization.Tests.MonetaryValueTests;

[PrefixTestClass]
public class CurrencyProperty
{
    [TestMethod]
    public void Default_Throws()
    {
        Should.Throw<InvalidOperationException>(() => MonetaryValue.Default.Currency);
    }
}