<div class="article">

# Singulink.Globalization.Currency

**Singulink.Globalization.Currency** is a .NET library that provides types like `Currency`, `MonetaryValue` and a range of collections and interfaces that make it easy to work with money and currencies in your applications. The library is well-documented and follows the same design principles as built-in .NET types and collections. It has been painstakingly tested and optimized to ensure that it is both fast and reliable.

This package is part of our **Singulink Libraries** collection. Visit https://github.com/Singulink to see our full list of publicly available libraries and other open-source projects.

### Packages

- **Singulink.Globalization.Currency**: The core library. Its default currency registry is built from the globalization data that ships with the runtime.
- **Singulink.Globalization.Currency.Cldr**: Optional package that provides an up-to-date currency registry built from Unicode CLDR data, including localized names and symbols for every locale, currencies that are not tied to a region, and cash rounding rules. Behaves identically on every runtime and operating system.

### Key Features

✔️ Wide range of string formatting options for every scenario (including `ISpanFormattable` support)  
✔️ Four types of money bag collections for working with multiple currencies at once  
✔️ Collection expression/literal syntax support  
✔️ Builds currency data from built-in system globalization data, or from embedded Unicode CLDR data with the optional CLDR package  
✔️ Standard and cash rounding rules for every currency (cash rounding requires the CLDR package)  
✔️ Support for custom currency registries, i.e. for cryptocurrency support  
✔️ Generic math support (.NET)  
✔️ Extensive `ReadOnlySpan<char>` lookup support (.NET 9+)  
✔️ Full AOT and WinRT support  
✔️ Extensive test coverage  

## Installation

The packages are available on NuGet - simply install the `Singulink.Globalization.Currency` package, and optionally the `Singulink.Globalization.Currency.Cldr` package for CLDR-based currency data.

**Supported Runtimes**: Everywhere .NET Standard 2.0 is supported, including:
- .NET
- .NET Framework
- Mono / Xamarin

End-of-life runtime versions that are no longer officially supported are not tested or supported by this library.

## Information and Links

Here are some additional links to get you started:

- [API Documentation](api/Singulink.Globalization.Currency.yml) - Browse the fully documented API here.
- [Chat on Discord](https://discord.gg/EkQhJFsBu6) - Have questions or want to discuss the library? This is the place for all Singulink project discussions.
- [GitHub Repo](https://github.com/Singulink/Singulink.Globalization.Currency) - File issues, contribute pull requests or check out the code for yourself!

</div>