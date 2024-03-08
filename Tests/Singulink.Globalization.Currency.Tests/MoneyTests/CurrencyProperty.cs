namespace Singulink.Globalization.Tests.MoneyTests;

[PrefixTestClass]
public class CurrencyProperty
{
    [TestMethod]
    public void Default_Throws()
    {
        Should.Throw<InvalidOperationException>(() => Money.Default.Currency);
    }
}