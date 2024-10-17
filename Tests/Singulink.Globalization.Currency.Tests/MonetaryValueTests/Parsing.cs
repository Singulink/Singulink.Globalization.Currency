using System.Globalization;

namespace Singulink.Globalization.Tests.MonetaryValueTests;

[PrefixTestClass]
public class Parsing
{
    [TestMethod]
    public void Parse_LocalPositiveValues_RoundtripsProperly()
    {
        foreach (var culture in GetSpecificCultures())
        {
            CultureInfo.CurrentCulture = culture;
            Currency.TryGetLocalCurrency(out var currency).ShouldBeTrue();

            var value = MonetaryValue.Create(1234.56m, currency);
            string valueString = value.ToString("C");

            MonetaryValue.Parse(valueString, MonetaryStyles.LocalSymbol).ShouldBe(value, $"Parsed string: '{valueString}'");
        }
    }

    [TestMethod]
    public void Parse_LocalNegativeValues_RoundtripsProperly()
    {
        foreach (var culture in GetSpecificCultures())
        {
            CultureInfo.CurrentCulture = culture;
            Currency.TryGetLocalCurrency(out var currency).ShouldBeTrue();

            var value = MonetaryValue.Create(-1234.56m, currency);
            string valueString = value.ToString("C");

            MonetaryValue.Parse(valueString, MonetaryStyles.LocalSymbol).ShouldBe(value, $"Parsed string: '{valueString}'");
        }
    }

    [TestMethod]
    public void Parse_GeneralPositiveValues_RoundtripsProperly()
    {
        foreach (var culture in GetSpecificCultures())
        {
            CurrencyRegistry.Default.TryGetLocalCurrency(culture, out var currency).ShouldBeTrue();

            var value = MonetaryValue.Create(1234.56m, currency);
            string valueString = value.ToString();

            MonetaryValue.Parse(valueString, MonetaryStyles.CurrencyCode).ShouldBe(value, $"Parsed string: '{valueString}'");
        }
    }

    [TestMethod]
    public void Parse_GeneralNegativeValues_RoundtripsProperly()
    {
        foreach (var culture in GetSpecificCultures())
        {
            CurrencyRegistry.Default.TryGetLocalCurrency(culture, out var currency).ShouldBeTrue();

            var value = MonetaryValue.Create(-1234.56m, currency);
            string valueString = value.ToString();

            MonetaryValue.Parse(valueString, MonetaryStyles.CurrencyCode).ShouldBe(value, $"Parsed string: '{valueString}'");
        }
    }

    private static IEnumerable<CultureInfo> GetSpecificCultures()
        => CultureInfo.GetCultures(CultureTypes.SpecificCultures).Where(c => new RegionInfo(c.Name).ISOCurrencySymbol.Length is 3);
}
