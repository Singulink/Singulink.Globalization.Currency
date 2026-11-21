using System.Runtime.CompilerServices;
using BenchmarkDotNet.Running;
using NodaMoney;
using Singulink.Globalization;

Console.WriteLine(
    $"Size in bytes: decimal {Unsafe.SizeOf<decimal>()}, NodaMoney Money {Unsafe.SizeOf<Money>()}, NodaMoney FastMoney {Unsafe.SizeOf<FastMoney>()}, " +
    $"MonetaryValue {Unsafe.SizeOf<MonetaryValue>()}");

if (args is ["sizes"])
    return;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args.Length > 0 ? args : ["--filter", "*"]);
