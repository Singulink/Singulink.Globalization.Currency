using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using NodaMoney;
using Singulink.Globalization;

namespace Singulink.Globalization.Benchmarks;

/// <summary>
/// Arithmetic on 1,000 USD amounts with two decimals, for a plain decimal, NodaMoney's Money and FastMoney, and this library's MonetaryValue. Each result
/// is the time for one value. NodaMoney's types run in their default context, which rounds a Money to the currency after every operation.
/// </summary>
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class MoneyBenchmarks
{
    private const int Count = 1000;
    private const decimal Rate = 1.0825m;

    private readonly decimal[] _decimalA = new decimal[Count];
    private readonly decimal[] _decimalB = new decimal[Count];
    private readonly decimal[] _decimalResult = new decimal[Count];

    private readonly Money[] _moneyA = new Money[Count];
    private readonly Money[] _moneyB = new Money[Count];
    private readonly Money[] _moneyResult = new Money[Count];

    private readonly FastMoney[] _fastMoneyA = new FastMoney[Count];
    private readonly FastMoney[] _fastMoneyB = new FastMoney[Count];
    private readonly FastMoney[] _fastMoneyResult = new FastMoney[Count];

    private readonly MonetaryValue[] _valueA = new MonetaryValue[Count];
    private readonly MonetaryValue[] _valueB = new MonetaryValue[Count];
    private readonly MonetaryValue[] _valueResult = new MonetaryValue[Count];

    private CurrencyInfo _usdInfo = null!;
    private Singulink.Globalization.Currency _usd = null!;

    [GlobalSetup]
    public void Setup()
    {
        _usdInfo = CurrencyInfo.FromCode("USD");
        _usd = Singulink.Globalization.Currency.GetCurrency("USD");

        var random = new Random(12345);

        for (int i = 0; i < Count; i++)
        {
            _decimalA[i] = random.Next(1, 1_000_001) / 100m;
            _decimalB[i] = random.Next(1, 1_000_001) / 100m;

            _moneyA[i] = new Money(_decimalA[i], _usdInfo);
            _moneyB[i] = new Money(_decimalB[i], _usdInfo);

            _fastMoneyA[i] = new FastMoney(_decimalA[i], _usdInfo);
            _fastMoneyB[i] = new FastMoney(_decimalB[i], _usdInfo);

            _valueA[i] = new MonetaryValue(_decimalA[i], _usd);
            _valueB[i] = new MonetaryValue(_decimalB[i], _usd);
        }
    }

    // Create a value from a decimal and a currency

    [BenchmarkCategory("Create"), Benchmark(OperationsPerInvoke = Count)]
    public Money[] Create_Money()
    {
        var a = _decimalA;
        var r = _moneyResult;
        var usd = _usdInfo;

        for (int i = 0; i < a.Length; i++)
            r[i] = new Money(a[i], usd);

        return r;
    }

    [BenchmarkCategory("Create"), Benchmark(OperationsPerInvoke = Count)]
    public FastMoney[] Create_FastMoney()
    {
        var a = _decimalA;
        var r = _fastMoneyResult;
        var usd = _usdInfo;

        for (int i = 0; i < a.Length; i++)
            r[i] = new FastMoney(a[i], usd);

        return r;
    }

    [BenchmarkCategory("Create"), Benchmark(OperationsPerInvoke = Count)]
    public MonetaryValue[] Create_MonetaryValue()
    {
        var a = _decimalA;
        var r = _valueResult;
        var usd = _usd;

        for (int i = 0; i < a.Length; i++)
            r[i] = new MonetaryValue(a[i], usd);

        return r;
    }

    // Add two values

    [BenchmarkCategory("Add"), Benchmark(OperationsPerInvoke = Count)]
    public decimal[] Add_Decimal()
    {
        var a = _decimalA;
        var b = _decimalB;
        var r = _decimalResult;

        for (int i = 0; i < a.Length; i++)
            r[i] = a[i] + b[i];

        return r;
    }

    [BenchmarkCategory("Add"), Benchmark(OperationsPerInvoke = Count)]
    public Money[] Add_Money()
    {
        var a = _moneyA;
        var b = _moneyB;
        var r = _moneyResult;

        for (int i = 0; i < a.Length; i++)
            r[i] = a[i] + b[i];

        return r;
    }

    [BenchmarkCategory("Add"), Benchmark(OperationsPerInvoke = Count)]
    public FastMoney[] Add_FastMoney()
    {
        var a = _fastMoneyA;
        var b = _fastMoneyB;
        var r = _fastMoneyResult;

        for (int i = 0; i < a.Length; i++)
            r[i] = a[i] + b[i];

        return r;
    }

    [BenchmarkCategory("Add"), Benchmark(OperationsPerInvoke = Count)]
    public MonetaryValue[] Add_MonetaryValue()
    {
        var a = _valueA;
        var b = _valueB;
        var r = _valueResult;

        for (int i = 0; i < a.Length; i++)
            r[i] = a[i] + b[i];

        return r;
    }

    // Total of all values

    [BenchmarkCategory("Sum"), Benchmark(OperationsPerInvoke = Count)]
    public decimal Sum_Decimal()
    {
        decimal total = 0;

        foreach (decimal value in _decimalA)
            total += value;

        return total;
    }

    [BenchmarkCategory("Sum"), Benchmark(OperationsPerInvoke = Count)]
    public Money Sum_Money()
    {
        var total = new Money(0m, _usdInfo);

        foreach (var value in _moneyA)
            total += value;

        return total;
    }

    [BenchmarkCategory("Sum"), Benchmark(OperationsPerInvoke = Count)]
    public FastMoney Sum_FastMoney()
    {
        var total = new FastMoney(0m, _usdInfo);

        foreach (var value in _fastMoneyA)
            total += value;

        return total;
    }

    [BenchmarkCategory("Sum"), Benchmark(OperationsPerInvoke = Count)]
    public MonetaryValue Sum_MonetaryValue()
    {
        var total = MonetaryValue.Default;

        foreach (var value in _valueA)
            total += value;

        return total;
    }

    // Multiply by a rate. A Money rounds the result to the currency and a FastMoney to four decimals; a MonetaryValue keeps the full result unless Round
    // is called.

    [BenchmarkCategory("Multiply"), Benchmark(OperationsPerInvoke = Count)]
    public decimal[] Multiply_Decimal()
    {
        var a = _decimalA;
        var r = _decimalResult;

        for (int i = 0; i < a.Length; i++)
            r[i] = a[i] * Rate;

        return r;
    }

    [BenchmarkCategory("Multiply"), Benchmark(OperationsPerInvoke = Count)]
    public Money[] Multiply_Money()
    {
        var a = _moneyA;
        var r = _moneyResult;

        for (int i = 0; i < a.Length; i++)
            r[i] = a[i] * Rate;

        return r;
    }

    [BenchmarkCategory("Multiply"), Benchmark(OperationsPerInvoke = Count)]
    public FastMoney[] Multiply_FastMoney()
    {
        var a = _fastMoneyA;
        var r = _fastMoneyResult;

        for (int i = 0; i < a.Length; i++)
            r[i] = a[i] * Rate;

        return r;
    }

    [BenchmarkCategory("Multiply"), Benchmark(OperationsPerInvoke = Count)]
    public MonetaryValue[] Multiply_MonetaryValue()
    {
        var a = _valueA;
        var r = _valueResult;

        for (int i = 0; i < a.Length; i++)
            r[i] = a[i] * Rate;

        return r;
    }

    [BenchmarkCategory("Multiply"), Benchmark(OperationsPerInvoke = Count)]
    public MonetaryValue[] Multiply_MonetaryValue_Rounded()
    {
        var a = _valueA;
        var r = _valueResult;

        for (int i = 0; i < a.Length; i++)
            r[i] = (a[i] * Rate).Round();

        return r;
    }

    // Divide by a number

    [BenchmarkCategory("Divide"), Benchmark(OperationsPerInvoke = Count)]
    public decimal[] Divide_Decimal()
    {
        var a = _decimalA;
        var r = _decimalResult;

        for (int i = 0; i < a.Length; i++)
            r[i] = a[i] / 3m;

        return r;
    }

    [BenchmarkCategory("Divide"), Benchmark(OperationsPerInvoke = Count)]
    public Money[] Divide_Money()
    {
        var a = _moneyA;
        var r = _moneyResult;

        for (int i = 0; i < a.Length; i++)
            r[i] = a[i] / 3m;

        return r;
    }

    [BenchmarkCategory("Divide"), Benchmark(OperationsPerInvoke = Count)]
    public FastMoney[] Divide_FastMoney()
    {
        var a = _fastMoneyA;
        var r = _fastMoneyResult;

        for (int i = 0; i < a.Length; i++)
            r[i] = a[i] / 3m;

        return r;
    }

    [BenchmarkCategory("Divide"), Benchmark(OperationsPerInvoke = Count)]
    public MonetaryValue[] Divide_MonetaryValue()
    {
        var a = _valueA;
        var r = _valueResult;

        for (int i = 0; i < a.Length; i++)
            r[i] = a[i] / 3m;

        return r;
    }

    [BenchmarkCategory("Divide"), Benchmark(OperationsPerInvoke = Count)]
    public MonetaryValue[] Divide_MonetaryValue_Rounded()
    {
        var a = _valueA;
        var r = _valueResult;

        for (int i = 0; i < a.Length; i++)
            r[i] = (a[i] / 3m).Round();

        return r;
    }

    // Compare two values

    [BenchmarkCategory("Compare"), Benchmark(OperationsPerInvoke = Count)]
    public int Compare_Decimal()
    {
        var a = _decimalA;
        var b = _decimalB;
        int count = 0;

        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] < b[i])
                count++;
        }

        return count;
    }

    [BenchmarkCategory("Compare"), Benchmark(OperationsPerInvoke = Count)]
    public int Compare_Money()
    {
        var a = _moneyA;
        var b = _moneyB;
        int count = 0;

        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] < b[i])
                count++;
        }

        return count;
    }

    [BenchmarkCategory("Compare"), Benchmark(OperationsPerInvoke = Count)]
    public int Compare_FastMoney()
    {
        var a = _fastMoneyA;
        var b = _fastMoneyB;
        int count = 0;

        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] < b[i])
                count++;
        }

        return count;
    }

    [BenchmarkCategory("Compare"), Benchmark(OperationsPerInvoke = Count)]
    public int Compare_MonetaryValue()
    {
        var a = _valueA;
        var b = _valueB;
        int count = 0;

        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] < b[i])
                count++;
        }

        return count;
    }
}
