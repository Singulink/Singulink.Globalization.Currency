using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Singulink.Enums;

namespace Singulink.Globalization;

/// <summary>
/// Converts a <see cref="MonetaryValue"/> to and from JSON as an object with an amount and a currency code, i.e. <c>{ "amount": 12.34, "currency": "USD"
/// }</c>, or as a string in the round-trip format, i.e. <c>"USD 12.34"</c>. Currencies are read from the currency registry that the converter was created
/// with.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MonetaryValue"/> is annotated with this converter, so values serialize in the object form with the <see cref="CurrencyRegistry.Default"/>
/// registry without any setup. To write the string form or to read values from a different registry, add an instance created with one of the other
/// constructors to <see cref="JsonSerializerOptions.Converters"/>, which takes precedence over the converter on the type.</para>
/// <para>
/// Reading accepts the object and the string form, whatever form the converter writes. A string must hold the round-trip format <c>"O"</c> of <see
/// cref="MonetaryValue.ToString(string?, IFormatProvider?)"/>: the currency code, one space and the amount with the invariant culture, such as <c>"USD
/// 12.30"</c> or <c>"USD -0.5"</c>, with any number of decimals. Culture-formatted text, such as <c>"$12.30"</c> or <c>"USD 1,234.50"</c>, throws a <see
/// cref="JsonException"/>. Every form reads back as an equal value.</para>
/// <para>
/// In the object form, the amount is written as a JSON number with the same decimals as the round-trip format: the currency's decimal digits, or more when the
/// amount needs them, so equal values write identical JSON. With <see cref="JsonNumberHandling.WriteAsString"/> in <see
/// cref="JsonSerializerOptions.NumberHandling"/>, the amount is written as a string with those decimals instead, and with <see
/// cref="JsonNumberHandling.AllowReadingFromString"/> an amount that is a string is read with the invariant culture. The currency is written as its <see
/// cref="Currency.CurrencyCode"/>, which does not use the registry.</para>
/// <para>
/// A reader that uses double-precision numbers, such as JavaScript, keeps only 15 significant digits of a JSON number. So in the <see
/// cref="MonetaryJsonFormat.Object"/> form, writing an amount with more than 15 significant digits as a number throws an <see cref="ArgumentException"/>.
/// Round the value, write amounts as strings with <see cref="JsonNumberHandling.WriteAsString"/>, use the <see cref="MonetaryJsonFormat.Text"/> form, or
/// use <see cref="MonetaryJsonFormat.LossyNumberObject"/> if every reader keeps all the digits. Reading never limits the digits.</para>
/// <para>
/// The <see cref="MonetaryValue.Default"/> value, which has no currency, is written as a zero with a <c>null</c> currency in the object form (<c>{ "amount": 0,
/// "currency": null }</c>) and as <c>"0"</c> in the string form, and both read as the default value. A non-zero amount with a <c>null</c> currency throws a
/// <see cref="JsonException"/>.</para>
/// <para>
/// The property names are <c>amount</c> and <c>currency</c>. If <see cref="JsonSerializerOptions.PropertyNamingPolicy"/> is set, the names are the policy's
/// conversion of <c>Amount</c> and <c>Currency</c> instead. Properties may appear in any order, names are matched case-insensitively if <see
/// cref="JsonSerializerOptions.PropertyNameCaseInsensitive"/> is set, and unknown properties are skipped. A missing amount or currency, or a currency code
/// that is not in the registry, throws a <see cref="JsonException"/>.</para>
/// <para>
/// Monetary values are also supported as dictionary keys, which are written in the round-trip format in every form.</para>
/// </remarks>
public sealed class MonetaryValueJsonConverter : JsonConverter<MonetaryValue>
{
    private const int StackBufferSize = 128;

    /// <summary>
    /// The most significant digits that a numeric amount can have in the object form. Every decimal number with at most 15 significant digits survives a
    /// round trip through a double-precision number, which is how JavaScript and most JSON parsers read a number by default, and a number with more may not.
    /// </summary>
    private const int MaxDoubleExactDigits = 15;

    private static readonly PropertyNames DefaultNames = new(null, JsonEncodedText.Encode("amount"), JsonEncodedText.Encode("currency"));

    private readonly CurrencyRegistry? _registry;
    private readonly MonetaryJsonFormat _format;
    private PropertyNames? _policyNames;

    /// <summary>
    /// Initializes a new instance of the <see cref="MonetaryValueJsonConverter"/> class that writes the object form and reads currencies from the <see
    /// cref="CurrencyRegistry.Default"/> registry.
    /// </summary>
    /// <remarks>
    /// The default registry is not accessed until the first value is read, so creating this converter does not prevent the default registry from being set at
    /// application startup.
    /// </remarks>
    public MonetaryValueJsonConverter() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MonetaryValueJsonConverter"/> class that writes the object form and reads currencies from the specified
    /// registry.
    /// </summary>
    /// <param name="registry">The registry that currency codes are looked up in when values are read.</param>
    public MonetaryValueJsonConverter(CurrencyRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MonetaryValueJsonConverter"/> class that writes the specified form and reads currencies from the <see
    /// cref="CurrencyRegistry.Default"/> registry.
    /// </summary>
    /// <param name="format">The form that values are written in. Reading accepts the object and the string form.</param>
    /// <remarks>
    /// The default registry is not accessed until the first value is read, so creating this converter does not prevent the default registry from being set at
    /// application startup.
    /// </remarks>
    /// <exception cref="ArgumentException">The format is not a defined <see cref="MonetaryJsonFormat"/> value.</exception>
    public MonetaryValueJsonConverter(MonetaryJsonFormat format) : this(null, format, true) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MonetaryValueJsonConverter"/> class that writes the specified form and reads currencies from the specified
    /// registry.
    /// </summary>
    /// <param name="registry">The registry that currency codes are looked up in when values are read.</param>
    /// <param name="format">The form that values are written in. Reading accepts the object and the string form.</param>
    /// <exception cref="ArgumentException">The format is not a defined <see cref="MonetaryJsonFormat"/> value.</exception>
    public MonetaryValueJsonConverter(CurrencyRegistry registry, MonetaryJsonFormat format) : this(registry, format, true) { }

    private MonetaryValueJsonConverter(CurrencyRegistry? registry, MonetaryJsonFormat format, bool validate)
    {
        if (validate && !format.IsValid())
            throw new ArgumentException($"An undefined {nameof(MonetaryJsonFormat)} value is being used.", nameof(format));

        _registry = registry;
        _format = format;
    }

    /// <summary>
    /// Creates a converter with the specified registry, or the default registry looked up when values are read if <paramref name="registry"/> is <see
    /// langword="null"/>.
    /// </summary>
    internal static MonetaryValueJsonConverter Create(CurrencyRegistry? registry, MonetaryJsonFormat format) => new(registry, format, true);

    /// <summary>
    /// Reads a monetary value from an object with an amount and a currency code or from a string in the round-trip format. The <see
    /// cref="MonetaryValue.Default"/> value is read from a zero amount with a <c>null</c> currency and from <c>"0"</c>.
    /// </summary>
    /// <exception cref="JsonException">The JSON is not an object or a string, the amount or the currency is missing or invalid, a string is not
    /// in the round-trip format, or the currency code is not in the converter's registry.</exception>
    public override MonetaryValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.String)
            return ReadRoundTripText(reader.GetString()!.AsSpan(), _registry ?? CurrencyRegistry.Default);

        if (reader.TokenType is not JsonTokenType.StartObject)
            throw new JsonException($"Expected a monetary value object or a string but found a token of type '{reader.TokenType}'.");

        var names = GetNames(options);

        decimal amount = 0;
        bool hasAmount = false;
        bool hasCurrency = false;
        Currency? currency = null;

        while (reader.Read() && reader.TokenType is JsonTokenType.PropertyName)
        {
            var property = MatchProperty(ref reader, names, options.PropertyNameCaseInsensitive);
            reader.Read();

            if (property is Property.Amount)
            {
                amount = ReadAmount(ref reader, names, options);
                hasAmount = true;
            }
            else if (property is Property.Currency)
            {
                hasCurrency = true;

                if (reader.TokenType is JsonTokenType.Null)
                {
                    currency = null;
                }
                else if (reader.TokenType is JsonTokenType.String)
                {
                    currency = CurrencyJsonConverter.ReadCurrencyCode(ref reader, _registry ?? CurrencyRegistry.Default);
                }
                else
                {
                    throw new JsonException(
                        $"Expected a currency code string or null for the '{names.Currency}' property but found a token of type '{reader.TokenType}'.");
                }
            }
            else
            {
                reader.Skip();
            }
        }

        if (!hasAmount)
            throw new JsonException($"The monetary value is missing the '{names.Amount}' property.");

        if (!hasCurrency)
            throw new JsonException($"The monetary value is missing the '{names.Currency}' property.");

        if (currency is null)
        {
            // A value without a currency can only be the default value, which has a zero amount.

            if (amount is not 0)
                throw new JsonException($"The monetary value has a null '{names.Currency}' and a non-zero amount. Only a zero amount can have no currency.");

            return default;
        }

        return new MonetaryValue(amount, currency);
    }

    /// <summary>
    /// Writes a monetary value in the converter's form. The <see cref="MonetaryValue.Default"/> value is written as a zero with a <c>null</c> currency in
    /// the object form and as <c>"0"</c> in the string form.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, MonetaryValue value, JsonSerializerOptions options)
    {
        if (_format is MonetaryJsonFormat.Text)
        {
            Span<char> buffer = stackalloc char[StackBufferSize];

            if (value.TryFormatRoundTrip(buffer, out int charsWritten))
                writer.WriteStringValue(buffer[..charsWritten]);
            else
                writer.WriteStringValue(value.ToRoundTripString());

            return;
        }

        var names = GetNames(options);

        writer.WriteStartObject();
        bool asString = (options.NumberHandling & JsonNumberHandling.WriteAsString) is not 0;

        if (!asString && _format is MonetaryJsonFormat.Object)
            EnsureNumberIsExact(value);

        WriteAmount(writer, names.Amount, value, asString);

        if (value.IsDefault)
            writer.WriteNull(names.Currency);
        else
            writer.WriteString(names.Currency, value.CurrencyOrDefault.CurrencyCode);

        writer.WriteEndObject();
    }

    /// <summary>
    /// Reads a monetary value from a property name, such as a dictionary key, in the round-trip format.
    /// </summary>
    /// <exception cref="JsonException">The property name is not in the round-trip format, or the currency code is not in the converter's registry.</exception>
    public override MonetaryValue ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ReadRoundTripText(reader.GetString()!.AsSpan(), _registry ?? CurrencyRegistry.Default);
    }

    /// <summary>
    /// Writes a monetary value as a property name, such as a dictionary key, in the round-trip format. The <see cref="MonetaryValue.Default"/> value is
    /// written as <c>"0"</c>.
    /// </summary>
    public override void WriteAsPropertyName(Utf8JsonWriter writer, MonetaryValue value, JsonSerializerOptions options)
    {
        Span<char> buffer = stackalloc char[StackBufferSize];

        if (value.TryFormatRoundTrip(buffer, out int charsWritten))
            writer.WritePropertyName(buffer[..charsWritten]);
        else
            writer.WritePropertyName(value.ToRoundTripString());
    }

    /// <summary>
    /// Reads text in the round-trip format: the currency code, one space and the amount as an optional minus sign, digits and an optional fraction with any
    /// number of decimals, or a zero amount alone for the default value, which the format writes as <c>"0"</c>.
    /// </summary>
    internal static MonetaryValue ReadRoundTripText(ReadOnlySpan<char> text, CurrencyRegistry registry)
    {
        int space = text.IndexOf(' ');

        if (space < 0 && IsRoundTripAmount(text) && IsZero(text))
            return default;

        if (space <= 0 || !IsRoundTripAmount(text[(space + 1)..]))
            ThrowNotRoundTrip(text);

        var code = text[..space];
        var amountText = text[(space + 1)..];

#if NET
        if (!decimal.TryParse(amountText, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal amount))
#else
        if (!decimal.TryParse(amountText.ToString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal amount))
#endif
        {
            throw new JsonException($"The amount in '{text.ToString()}' is outside the range of a decimal.");
        }

#if NET9_0_OR_GREATER
        if (!registry.TryGetCurrency(code, out var currency))
#else
        if (!registry.TryGetCurrency(code.ToString(), out var currency))
#endif
        {
            CurrencyJsonConverter.ThrowNotFound(code.ToString(), registry);
        }

        return new MonetaryValue(amount, currency);

        static bool IsRoundTripAmount(ReadOnlySpan<char> s)
        {
            if (s.Length > 0 && s[0] is '-')
                s = s[1..];

            int point = s.IndexOf('.');
            var whole = point < 0 ? s : s[..point];
            var fraction = point < 0 ? default : s[(point + 1)..];

            return whole.Length > 0 && IsDigits(whole) && (point < 0 || (fraction.Length > 0 && IsDigits(fraction)));
        }

        static bool IsZero(ReadOnlySpan<char> s)
        {
            foreach (char c in s)
            {
                if (c is not ('0' or '.' or '-'))
                    return false;
            }

            return true;
        }

        static bool IsDigits(ReadOnlySpan<char> s)
        {
            foreach (char c in s)
            {
                if (c is < '0' or > '9')
                    return false;
            }

            return true;
        }
    }

    [DoesNotReturn]
    private static void ThrowNotRoundTrip(ReadOnlySpan<char> text)
    {
        throw new JsonException($"The string '{text.ToString()}' is not a monetary value in the round-trip format, such as \"USD 12.34\".");
    }

    private static decimal ReadAmount(ref Utf8JsonReader reader, PropertyNames names, JsonSerializerOptions options)
    {
        decimal amount;

        if (reader.TokenType is JsonTokenType.String && (options.NumberHandling & JsonNumberHandling.AllowReadingFromString) is not 0)
        {
            string text = reader.GetString()!;

            if (!decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out amount))
                throw new JsonException($"The '{names.Amount}' property value '{text}' is not a number that a decimal can hold.");

            return amount;
        }

        if (reader.TokenType is not JsonTokenType.Number)
            throw new JsonException($"Expected a number for the '{names.Amount}' property but found a token of type '{reader.TokenType}'.");

        if (!reader.TryGetDecimal(out amount))
            throw new JsonException($"The '{names.Amount}' property value is outside the range of a decimal.");

        return amount;
    }

    /// <summary>
    /// Throws if the amount has more significant digits than a double-precision number keeps.
    /// </summary>
    private static void EnsureNumberIsExact(MonetaryValue value)
    {
        int digits = GetSignificantDigits(value.Amount);

        if (digits <= MaxDoubleExactDigits)
            return;

        throw new ArgumentException(
            $"The amount of {value.ToRoundTripString()} has {digits} significant digits, but a JSON reader that uses double-precision numbers, such as " +
            $"JavaScript, keeps only {MaxDoubleExactDigits}, so it would read a different amount. Round the value; write amounts as strings with " +
            $"{nameof(JsonNumberHandling)}.{nameof(JsonNumberHandling.WriteAsString)} or use {nameof(MonetaryJsonFormat)}.{nameof(MonetaryJsonFormat.Text)}; or " +
            $"use {nameof(MonetaryJsonFormat)}.{nameof(MonetaryJsonFormat.LossyNumberObject)} if every reader keeps all the digits of a number.",
            nameof(value));
    }

    /// <summary>
    /// Gets the significant digits of an amount: from its first digit that is not zero to its last digit that is not zero, wherever the decimal point is.
    /// Zero has none.
    /// </summary>
    private static int GetSignificantDigits(decimal amount)
    {
        // The integer mantissa holds every digit of the value. Without its trailing zeros, its length is the number of significant digits.

#if NET
        Span<int> bits = stackalloc int[4];
        decimal.GetBits(amount, bits);
#else
        int[] bits = decimal.GetBits(amount);
#endif

        decimal mantissa = new(bits[0], bits[1], bits[2], false, 0);

        if (mantissa is 0)
            return 0;

        while (mantissa % 10 is 0)
            mantissa /= 10;

        int digits = 0;

        while (mantissa >= 1)
        {
            mantissa = decimal.Truncate(mantissa / 10);
            digits++;
        }

        return digits;
    }

    /// <summary>
    /// Writes the amount property with the value's canonical amount text, the same text that the round-trip format uses, as a JSON number (written raw, so
    /// that it has exactly the decimals of that text) or as a JSON string.
    /// </summary>
    private static void WriteAmount(Utf8JsonWriter writer, JsonEncodedText name, MonetaryValue value, bool asString)
    {
        // The canonical amount has at most 29 digits, a decimal point and a sign. The default value's amount is "0", as the round-trip format writes it.
        Span<char> buffer = stackalloc char[64];
        int charsWritten;

        if (value.IsDefault)
        {
            buffer[0] = '0';
            charsWritten = 1;
        }
        else
        {
            bool formatted = value.TryFormatCanonicalAmount(buffer, out charsWritten);
            Debug.Assert(formatted, "Buffer is large enough for any canonical amount.");
        }

        var amountText = buffer[..charsWritten];

        if (asString)
        {
            writer.WriteString(name, amountText);
        }
        else
        {
            writer.WritePropertyName(name);
            writer.WriteRawValue(amountText);
        }
    }

    private static Property MatchProperty(ref Utf8JsonReader reader, PropertyNames names, bool caseInsensitive)
    {
        if (reader.ValueTextEquals(names.Amount.Value))
            return Property.Amount;

        if (reader.ValueTextEquals(names.Currency.Value))
            return Property.Currency;

        if (caseInsensitive)
        {
            string name = reader.GetString()!;

            if (string.Equals(name, names.Amount.Value, StringComparison.OrdinalIgnoreCase))
                return Property.Amount;

            if (string.Equals(name, names.Currency.Value, StringComparison.OrdinalIgnoreCase))
                return Property.Currency;
        }

        return Property.Unknown;
    }

    private PropertyNames GetNames(JsonSerializerOptions options)
    {
        var policy = options.PropertyNamingPolicy;

        if (policy is null)
            return DefaultNames;

        // Options cannot change once they are in use, so the names converted by the policy are cached for the last options seen.

        var names = _policyNames;

        if (names?.Options != options)
        {
            names = new PropertyNames(
                options,
                JsonEncodedText.Encode(policy.ConvertName(nameof(MonetaryValue.Amount)), options.Encoder),
                JsonEncodedText.Encode(policy.ConvertName(nameof(MonetaryValue.Currency)), options.Encoder));

            _policyNames = names;
        }

        return names;
    }

    private enum Property
    {
        Unknown,
        Amount,
        Currency,
    }

    private sealed class PropertyNames(JsonSerializerOptions? options, JsonEncodedText amount, JsonEncodedText currency)
    {
        public JsonSerializerOptions? Options { get; } = options;

        public JsonEncodedText Amount { get; } = amount;

        public JsonEncodedText Currency { get; } = currency;
    }
}
