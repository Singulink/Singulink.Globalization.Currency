// Generates the currency data embedded in the Singulink.Globalization.Currency.Cldr package from the Unicode CLDR JSON distribution.
//
// Usage: dotnet run --project Tools/CldrDataGenerator -- [cldr-version]
//
// The tool downloads the cldr-core and cldr-numbers-full npm packages for the specified CLDR version (defaults to the version currently referenced by
// the Cldr package), extracts the currency data and writes:
//
//   Source/Singulink.Globalization.Currency.Cldr/Data/CurrencyData.bin        (gzip compressed binary data, see CurrencyDataFormat.cs)          
//   Source/Singulink.Globalization.Currency.Cldr/CldrCurrencyData.Version.g.cs (CLDR version and release date constants)
//   Source/Singulink.Globalization.Currency.Cldr/Cldr.Version.props            (package version properties)
//
// When the CLDR version changes the package revision is reset to 0. When the data is regenerated for the same CLDR version the revision is preserved so
// that it can be bumped manually to republish the package with the same data.
//
// See https://github.com/unicode-org/cldr-json for the source data. CLDR data is licensed under the Unicode License (https://www.unicode.org/license.txt).

using System.Formats.Tar;
using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using Singulink.Globalization.Tools;

string repoRoot = FindRepoRoot();
string cldrProjectDir = Path.Combine(repoRoot, "Source", "Singulink.Globalization.Currency.Cldr");
string versionFilePath = Path.Combine(cldrProjectDir, "CldrCurrencyData.Version.g.cs");
string propsFilePath = Path.Combine(cldrProjectDir, "Cldr.Version.props");
string dataFilePath = Path.Combine(cldrProjectDir, "Data", "CurrencyData.bin");

var (currentVersion, currentRevision) = ReadCurrentVersion(propsFilePath);
string version = args.Length > 0 ? args[0] : currentVersion ?? throw new InvalidOperationException("No CLDR version specified and no existing version file found.");
int revision = version == currentVersion ? currentRevision : 0;

Console.WriteLine($"Generating currency data for CLDR version {version}...");

string workDir = Path.Combine(Path.GetTempPath(), "singulink-cldr", version);
Directory.CreateDirectory(workDir);

using var http = new HttpClient();

var releaseDate = await GetReleaseDateAsync(http, "cldr-core", version);
string coreDir = await DownloadAndExtractPackageAsync(http, "cldr-core", version, workDir);
string numbersDir = await DownloadAndExtractPackageAsync(http, "cldr-numbers-full", version, workDir);

var data = CldrCurrencyDataBuilder.Build(coreDir, numbersDir);

Directory.CreateDirectory(Path.GetDirectoryName(dataFilePath)!);
CurrencyDataFormat.Write(dataFilePath, version, data);
File.WriteAllText(versionFilePath, GenerateVersionFile(version, releaseDate));
File.WriteAllText(propsFilePath, GeneratePropsFile(version, releaseDate, revision));

Console.WriteLine($"Wrote {data.Currencies.Count} currencies and {data.Locales.Count} locales ({data.LocaleEntryCount} locale entries).");
Console.WriteLine($"  {dataFilePath} ({new FileInfo(dataFilePath).Length:N0} bytes)");
Console.WriteLine($"  {versionFilePath}");
Console.WriteLine($"  {propsFilePath} (package version {version}{(revision > 0 ? "." + revision : "")}, released {releaseDate:yyyy-MM-dd})");

static string FindRepoRoot()
{
    string? dir = AppContext.BaseDirectory;

    while (dir is not null)
    {
        if (File.Exists(Path.Combine(dir, "Singulink.Globalization.Currency.sln")))
            return dir;

        dir = Path.GetDirectoryName(dir);
    }

    throw new InvalidOperationException("Could not locate the repository root from the tool's base directory.");
}

static (string? Version, int Revision) ReadCurrentVersion(string propsFilePath)
{
    if (!File.Exists(propsFilePath))
        return (null, 0);

    string content = File.ReadAllText(propsFilePath);
    var versionMatch = Regex.Match(content, @"<CldrVersion>([^<]+)</CldrVersion>");
    var revisionMatch = Regex.Match(content, @"<CldrPackageRevision>(\d+)</CldrPackageRevision>");

    return (versionMatch.Success ? versionMatch.Groups[1].Value : null, revisionMatch.Success ? int.Parse(revisionMatch.Groups[1].Value, CultureInfo.InvariantCulture) : 0);
}

static async Task<DateTime> GetReleaseDateAsync(HttpClient http, string package, string version)
{
    // The npm registry records the publish time of every version of a package.

    using var response = await http.GetAsync($"https://registry.npmjs.org/{package}");
    response.EnsureSuccessStatusCode();

    using var doc = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());

    if (!doc.RootElement.GetProperty("time").TryGetProperty(version, out var time))
        throw new InvalidOperationException($"Version {version} of {package} was not found on the npm registry.");

    return time.GetDateTime().ToUniversalTime().Date;
}

static async Task<string> DownloadAndExtractPackageAsync(HttpClient http, string package, string version, string workDir)
{
    string extractDir = Path.Combine(workDir, package);

    if (Directory.Exists(Path.Combine(extractDir, "package")))
    {
        Console.WriteLine($"Using cached {package}@{version} from '{extractDir}'.");
        return Path.Combine(extractDir, "package");
    }

    string url = $"https://registry.npmjs.org/{package}/-/{package}-{version}.tgz";
    Console.WriteLine($"Downloading {url}...");

    using var response = await http.GetAsync(url);
    response.EnsureSuccessStatusCode();

    await using var tgzStream = await response.Content.ReadAsStreamAsync();
    await using var gzip = new GZipStream(tgzStream, CompressionMode.Decompress);

    Directory.CreateDirectory(extractDir);
    await TarFile.ExtractToDirectoryAsync(gzip, extractDir, overwriteFiles: true);

    return Path.Combine(extractDir, "package");
}

static string GenerateVersionFile(string version, DateTime releaseDate)
{
    return $$"""
        // <auto-generated>
        // This file is generated by Tools/CldrDataGenerator. Do not edit it manually.
        // </auto-generated>

        namespace Singulink.Globalization;

        partial class CldrCurrencyData
        {
            /// <summary>
            /// Gets the version of the Unicode Common Locale Data Repository (CLDR) that the currency data in this package was generated from.
            /// </summary>
            public const string CldrVersion = "{{version}}";

            /// <summary>
            /// Gets the date that the CLDR version the currency data in this package was generated from was released.
            /// </summary>
            public static DateTime CldrReleaseDate { get; } = new DateTime({{releaseDate.Year}}, {{releaseDate.Month}}, {{releaseDate.Day}}, 0, 0, 0, DateTimeKind.Utc);
        }

        """.ReplaceLineEndings("\r\n");
}

static string GeneratePropsFile(string version, DateTime releaseDate, int revision)
{
    return $"""
        <!-- This file is generated by Tools/CldrDataGenerator. Only CldrPackageRevision should be edited manually (to republish the same data). -->
        <Project>
          <PropertyGroup>
            <CldrVersion>{version}</CldrVersion>
            <CldrReleaseDate>{releaseDate:yyyy-MM-dd}</CldrReleaseDate>
            <CldrPackageRevision>{revision}</CldrPackageRevision>
          </PropertyGroup>
        </Project>

        """.ReplaceLineEndings("\r\n");
}
