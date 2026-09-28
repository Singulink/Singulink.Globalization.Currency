<div class="article">

# Singulink.Globalization.Currency

**Singulink.Globalization.Currency** is a .NET library that provides types like `Currency`, `MonetaryValue` and a range of collections and interfaces that make it easy to work with money and currencies in your applications. The library is well-documented and follows the same design principles as built-in .NET types and collections. It has been painstakingly tested and optimized to ensure that it is both fast and reliable.

This package is part of our **Singulink Libraries** collection. Visit https://github.com/Singulink to see our full list of publicly available libraries and other open-source projects.

### Packages

- **Singulink.Globalization.Currency**: The core library. Its default currency registry is built from the globalization data that ships with the runtime.
- **Singulink.Globalization.Currency.Cldr**: Optional data package with up-to-date currency data from Unicode CLDR, including localized names and symbols for every locale, currencies that are not tied to a region, and cash rounding rules. Behaves identically on every runtime and operating system. It does not depend on the core library so it can always be updated to the latest CLDR release, and its version mirrors the CLDR version.
- **Singulink.Globalization.Currency.DataProviders**: The `ICurrencyDataProvider` abstraction that data packages implement. Referenced by the other two packages, so it does not normally need to be referenced directly.

### Key Features

✔️ Immutable monetary values with currency-safe arithmetic, comparison and generic math support  
✔️ Rounding and cash rounding policies for every currency, plus allocation that never loses a cent  
✔️ Wide range of string formatting options for every scenario (including `ISpanFormattable` support)  
✔️ Culture-aware parsing of currency codes and symbols  
✔️ Four types of money bag collections for working with multiple currencies at once  
✔️ Collection expression/literal syntax support  
✔️ Builds currency data from built-in system globalization data, or from embedded Unicode CLDR data with the optional CLDR package  
✔️ Support for custom currencies, registries and data providers, i.e. for cryptocurrency support  
✔️ Extensive `ReadOnlySpan<char>` lookup support (.NET 9+)  
✔️ Full AOT and WinRT support  
✔️ Extensive test coverage  

### Installation

The packages are available on NuGet - simply install the `Singulink.Globalization.Currency` package, and optionally the `Singulink.Globalization.Currency.Cldr` package for CLDR-based currency data.

**Supported Runtimes**: Everywhere .NET Standard 2.0 is supported, including:
- .NET
- .NET Framework
- Mono / Xamarin

End-of-life runtime versions that are no longer officially supported are not tested or supported by this library.

## Information and Links

Here are some additional links to get you started:

- [Getting Started](articles/guides/getting-started.md) - Visit here first for a quick walkthrough of the core types.
- [Guides](articles/guides/toc.yml) - In-depth articles on currencies, values, rounding, formatting, parsing, money bags and data providers.
- [Concepts](articles/concepts/toc.yml) - How currency identity and localization work under the hood.
- [API Documentation](api/Singulink.Globalization.Currency.yml) - Browse the fully documented API here.
- [Chat on Discord](https://discord.gg/EkQhJFsBu6) - Have questions or want to discuss the library? This is the place for all Singulink project discussions.
- [GitHub Repo](https://github.com/Singulink/Singulink.Globalization.Currency) - File issues, contribute pull requests or check out the code for yourself!

</div>
