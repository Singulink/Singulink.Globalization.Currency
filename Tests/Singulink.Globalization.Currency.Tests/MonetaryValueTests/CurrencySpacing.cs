using System.Globalization;

namespace Singulink.Globalization.Tests.MonetaryValueTests;

/// <summary>
/// Tests for the currency spacing rule of UTS #35 (root <c>currencySpacing</c>) in the symbol formats: a no-break space goes between the symbol and the
/// number where the pattern puts them against each other and the symbol's character next to the number is neither a symbol character nor a separator.
/// </summary>
[PrefixTestClass]
public class CurrencySpacing
{
    private const char Sp = ' ';

    // Symbols with each kind of edge. The symbols are the same in every culture, since the currencies have no localizer.
    private static readonly Currency Letters = new("AAA", "Letters", "ABC");                // letters at both edges
    private static readonly Currency Dollar = new("BBB", "Dollar", "$");                    // a currency symbol at both edges
    private static readonly Currency LettersDollar = new("CCC", "Letters dollar", "CA$");   // a letter first, a currency symbol last
    private static readonly Currency DollarLetters = new("DDD", "Dollar letters", "$CA");   // a currency symbol first, a letter last
    private static readonly Currency Period = new("EEE", "Period", "Bs.");                  // a letter first, a period last
    private static readonly Currency Euro = new("FFF", "Euro", "€");

    private static readonly CurrencyRegistry Registry = new("Spacing", [Letters, Dollar, LettersDollar, DollarLetters, Period, Euro]);

    // Symbols that format but cannot be parsed: a digit at the end, a letter outside the BMP, an emoji.
    private static readonly Currency Digit = new("GGG", "Digit", "X1");
    private static readonly Currency SupplementaryLetter = new("HHH", "Supplementary letter", "\U0001D538");  // MATHEMATICAL DOUBLE-STRUCK CAPITAL A
    private static readonly Currency Emoji = new("III", "Emoji", "\U0001F4B0");                              // MONEY BAG

    /// <summary>
    /// The .NET positive currency patterns (<see cref="NumberFormatInfo.CurrencyPositivePattern"/>) with the expected result for <c>ABC 1.50</c>.
    /// </summary>
    private static readonly string[] LettersPositive = [$"ABC{Sp}1.50", $"1.50{Sp}ABC", $"ABC{Sp}1.50", $"1.50{Sp}ABC"];

    /// <summary>
    /// The .NET negative currency patterns (<see cref="NumberFormatInfo.CurrencyNegativePattern"/>) with the expected result for <c>ABC -1.50</c>. A space is
    /// inserted only where the symbol is directly next to the number: a sign between them (patterns 2 and 6) leaves the text as it is.
    /// </summary>
    private static readonly string[] LettersNegative = [
        $"(ABC{Sp}1.50)",   // 0: ($n)
        $"-ABC{Sp}1.50",    // 1: -$n
        "ABC-1.50",         // 2: $-n
        $"ABC{Sp}1.50-",    // 3: $n-
        $"(1.50{Sp}ABC)",   // 4: (n$)
        $"-1.50{Sp}ABC",    // 5: -n$
        "1.50-ABC",         // 6: n-$
        $"1.50{Sp}ABC-",    // 7: n$-
        $"-1.50{Sp}ABC",    // 8: -n $
        $"-ABC{Sp}1.50",    // 9: -$ n
        $"1.50{Sp}ABC-",    // 10: n $-
        $"ABC{Sp}1.50-",    // 11: $ n-
        $"ABC{Sp}-1.50",    // 12: $ -n
        $"1.50-{Sp}ABC",    // 13: n- $
        $"(ABC{Sp}1.50)",   // 14: ($ n)
        $"(1.50{Sp}ABC)",   // 15: (n $)
        $"ABC-{Sp}1.50",    // 16: $- n
    ];

    /// <summary>
    /// The same negative patterns for <c>$ -1.50</c>, where the symbol's edges are currency symbols and no space is inserted.
    /// </summary>
    private static readonly string[] DollarNegative = [
        "($1.50)", "-$1.50", "$-1.50", "$1.50-", "(1.50$)", "-1.50$", "1.50-$", "1.50$-", $"-1.50{Sp}$", $"-${Sp}1.50", $"1.50{Sp}$-", $"${Sp}1.50-",
        $"${Sp}-1.50", $"1.50-{Sp}$", $"(${Sp}1.50)", $"(1.50{Sp}$)", $"$-{Sp}1.50",
    ];

    /// <summary>
    /// The number of negative patterns that the runtime supports: .NET Framework has patterns 0 to 15, and .NET added pattern 16.
    /// </summary>
    private static readonly int NegativePatternCount = GetNegativePatternCount();

    private static int GetNegativePatternCount()
    {
        try
        {
            ((NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone()).CurrencyNegativePattern = 16;
            return 17;
        }
        catch (ArgumentOutOfRangeException)
        {
            return 16;
        }
    }

    private static NumberFormatInfo Format(int positivePattern, int negativePattern)
    {
        var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.CurrencyPositivePattern = positivePattern;
        nfi.CurrencyNegativePattern = negativePattern;
        nfi.CurrencyDecimalSeparator = ".";
        nfi.CurrencyGroupSeparator = ",";
        nfi.NegativeSign = "-";
        return nfi;
    }

    [TestMethod]
    public void LetterSymbol_PositivePatterns()
    {
        for (int p = 0; p < LettersPositive.Length; p++)
            new MonetaryValue(1.5m, Letters).ToString("C", Format(p, 0)).ShouldBe(LettersPositive[p], $"pattern {p}");
    }

    [TestMethod]
    public void LetterSymbol_NegativePatterns()
    {
        for (int p = 0; p < NegativePatternCount; p++)
            new MonetaryValue(-1.5m, Letters).ToString("C", Format(0, p)).ShouldBe(LettersNegative[p], $"pattern {p}");
    }

    [TestMethod]
    public void SymbolCharacterEdges_AreUnchanged()
    {
        for (int p = 0; p < NegativePatternCount; p++)
            new MonetaryValue(-1.5m, Dollar).ToString("C", Format(0, p)).ShouldBe(DollarNegative[p], $"pattern {p}");

        new MonetaryValue(1.5m, Dollar).ToString("C", Format(0, 0)).ShouldBe("$1.50");
        new MonetaryValue(1.5m, Dollar).ToString("C", Format(1, 0)).ShouldBe("1.50$");
        new MonetaryValue(1.5m, Euro).ToString("C", Format(0, 0)).ShouldBe("€1.50");
        new MonetaryValue(1.5m, Euro).ToString("C", Format(1, 0)).ShouldBe("1.50€");
        new MonetaryValue(1.5m, Emoji).ToString("C", Format(0, 0)).ShouldBe("\U0001F4B01.50");
        new MonetaryValue(1.5m, Emoji).ToString("C", Format(1, 0)).ShouldBe("1.50\U0001F4B0");
    }

    [TestMethod]
    public void OnlyTheEdgeNextToTheNumberCounts()
    {
        // "CA$" ends with a currency symbol and starts with a letter.
        new MonetaryValue(1.5m, LettersDollar).ToString("C", Format(0, 0)).ShouldBe("CA$1.50");
        new MonetaryValue(1.5m, LettersDollar).ToString("C", Format(1, 0)).ShouldBe($"1.50{Sp}CA$");

        // "$CA" starts with a currency symbol and ends with a letter.
        new MonetaryValue(1.5m, DollarLetters).ToString("C", Format(0, 0)).ShouldBe($"$CA{Sp}1.50");
        new MonetaryValue(1.5m, DollarLetters).ToString("C", Format(1, 0)).ShouldBe("1.50$CA");
    }

    [TestMethod]
    public void PeriodDigitAndSupplementaryEdges_GetSpace()
    {
        new MonetaryValue(1.5m, Period).ToString("C", Format(0, 0)).ShouldBe($"Bs.{Sp}1.50");
        new MonetaryValue(1.5m, Period).ToString("C", Format(1, 0)).ShouldBe($"1.50{Sp}Bs.");
        new MonetaryValue(1.5m, Digit).ToString("C", Format(0, 0)).ShouldBe($"X1{Sp}1.50");
        new MonetaryValue(1.5m, Digit).ToString("C", Format(1, 0)).ShouldBe($"1.50{Sp}X1");
        new MonetaryValue(1.5m, SupplementaryLetter).ToString("C", Format(0, 0)).ShouldBe($"\U0001D538{Sp}1.50");
        new MonetaryValue(1.5m, SupplementaryLetter).ToString("C", Format(1, 0)).ShouldBe($"1.50{Sp}\U0001D538");
    }

    [TestMethod]
    public void CodeFormats_AreUnchanged()
    {
        // The position of the code comes from the language of the current culture when the provider is a NumberFormatInfo.
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        var nfi = Format(0, 1);

        new MonetaryValue(1.5m, Letters).ToString(null, nfi).ShouldBe($"AAA{Sp}1.50");
        new MonetaryValue(1.5m, Letters).ToString("I", nfi).ShouldBe($"AAA{Sp}1.50");
        new MonetaryValue(1.5m, Letters).ToString("R", nfi).ShouldBe($"1.50{Sp}AAA");
        new MonetaryValue(-1.5m, Letters).ToString("I", nfi).ShouldBe($"AAA{Sp}-1.50");
    }

    [TestMethod]
    public void RealCulture_FollowsItsPatterns()
    {
        var registry = CurrencyData.Load(new TestProvider()).Registry;
        var cad = registry["CAD"];
        var aud = registry["AUD"];

        foreach (string name in new[] { "en-AU", "en-US", "ja-JP", "de-DE", "fr-FR", "nl-NL", "es-ES" })
        {
            var culture = CultureInfo.GetCultureInfo(name);
            int positive = culture.NumberFormat.CurrencyPositivePattern;
            int negative = culture.NumberFormat.CurrencyNegativePattern;

            string Expected(string template) => template.Replace("ABC", "CAD").Replace("1.50", 1234.5m.ToString("N2", culture.NumberFormat));

            new MonetaryValue(1234.5m, cad).ToString("C", culture).ShouldBe(Expected(LettersPositive[positive]), name);
            new MonetaryValue(-1234.5m, cad).ToString("C", culture).ShouldBe(Expected(LettersNegative[negative]), name);
        }

        // "L" resolves to the symbol format for the culture's own currency, whose en-AU symbol in the test data is the letter "A".
        var enAU = CultureInfo.GetCultureInfo("en-AU");
        string number = 1234.5m.ToString("N2", enAU.NumberFormat);

        new MonetaryValue(1234.5m, aud).ToString("L", enAU).ShouldBe(LettersPositive[enAU.NumberFormat.CurrencyPositivePattern].Replace("ABC", "A").Replace("1.50", number));
        new MonetaryValue(1234.5m, cad).ToString("L", enAU).ShouldBe($"CAD{Sp}{number}");
    }

    [TestMethod]
    public void FormattedText_ParsesBack_WithAnySpacing()
    {
        // Text as formatting writes it (with a no-break space), with an ordinary space, and with no space, which the parser also accepts.

        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        try
        {
            foreach (var currency in new[] { Letters, Dollar, LettersDollar, DollarLetters, Period, Euro })
            {
                foreach (decimal amount in new[] { 1234.5m, -1234.5m })
                {
                    for (int p = 0; p < (amount < 0 ? NegativePatternCount : 4); p++)
                    {
                        var nfi = amount < 0 ? Format(0, p) : Format(p, 0);
                        var value = new MonetaryValue(amount, currency);
                        string text = value.ToString("C", nfi);

                        foreach (string variant in new[] { text, text.Replace(Sp, ' '), RemoveSpacingSpace(text, currency.Symbol) })
                        {
                            foreach (var style in new[] { MonetaryStyles.UnambiguousSymbols, MonetaryStyles.Any })
                            {
                                Registry.ParseMonetaryValue(variant, style, nfi).ShouldBe(value, $"'{variant}' pattern {p} style {style}");
                                Registry.TryParseMonetaryValue(variant.AsSpan(), style, nfi, out var parsed).ShouldBeTrue($"'{variant}' pattern {p}");
                                parsed.ShouldBe(value);
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [TestMethod]
    public void FormattedText_ParsesBack_WithLocalSymbol()
    {
        var registry = CurrencyData.Load(new TestProvider()).Registry;
        var aud = registry["AUD"];

        foreach (string name in new[] { "en-AU", "de-DE", "nl-NL" })
        {
            var culture = CultureInfo.GetCultureInfo(name);

            foreach (decimal amount in new[] { 1234.5m, -1234.5m })
            {
                // In the test data, CAD's symbol in each of these cultures is the letters "CAD".
                var value = new MonetaryValue(amount, registry["CAD"]);
                string text = value.ToString("C", culture);

                foreach (string variant in new[] { text, text.Replace(Sp, ' '), RemoveSpacingSpace(text, "CAD") })
                {
                    registry.ParseMonetaryValue(variant, MonetaryStyles.Any, culture).ShouldBe(value, $"'{variant}' {name}");
                    registry.ParseMonetaryValue(variant, MonetaryStyles.UnambiguousSymbols, culture).ShouldBe(value, $"'{variant}' {name}");
                }
            }
        }

        // AUD is en-AU's local currency, and its en-AU symbol in the test data is the letter "A".
        var enAU = CultureInfo.GetCultureInfo("en-AU");
        var local = new MonetaryValue(-1234.5m, aud);
        string localText = local.ToString("C", enAU);

        foreach (string variant in new[] { localText, localText.Replace(Sp, ' '), RemoveSpacingSpace(localText, "A") })
            registry.ParseMonetaryValue(variant, MonetaryStyles.LocalSymbol, enAU).ShouldBe(local, $"'{variant}'");
    }

    /// <summary>
    /// Removes the space that currency spacing puts between the symbol and the number.
    /// </summary>
    private static string RemoveSpacingSpace(string text, string symbol)
    {
        int index = text.IndexOf(symbol, StringComparison.Ordinal);

        if (index + symbol.Length < text.Length && text[index + symbol.Length] == Sp && index + symbol.Length + 1 < text.Length &&
            char.IsDigit(text[index + symbol.Length + 1]))
        {
            return text.Remove(index + symbol.Length, 1);
        }

        if (index > 1 && text[index - 1] == Sp && char.IsDigit(text[index - 2]))
            return text.Remove(index - 1, 1);

        return text;
    }

    private sealed class TestProvider : ICurrencyDataProvider
    {
        private static readonly string[] Cultures = ["en-AU", "en-US", "ja-JP", "de-DE", "fr-FR", "nl-NL", "es-ES"];

        public string Name => "Spacing";

        public string Version => "1.0";

        public DateTime? ReleaseDate => null;

        public IEnumerable<CurrencyDataEntry> GetCurrencies() =>
            [new CurrencyDataEntry("AUD", "Australian Dollar") { Symbol = "A$" }, new CurrencyDataEntry("CAD", "Canadian Dollar") { Symbol = "CA$" }];

        public IEnumerable<CurrencyLocalizationEntry> GetLocalizations() =>
            Cultures.SelectMany(c => new[] {
                new CurrencyLocalizationEntry(c, "CAD") { Symbol = "CAD" },
                new CurrencyLocalizationEntry(c, "AUD") { Symbol = "A" },
            });
    }
}
