namespace Singulink.Globalization;

internal sealed class InvariantCurrencyLocalizer(string name, string symbol) : ICurrencyLocalizer
{
    string ICurrencyLocalizer.GetName(Currency currency, CultureInfo culture) => name;

    string ICurrencyLocalizer.GetSymbol(Currency currency, CultureInfo culture) => symbol;
}
