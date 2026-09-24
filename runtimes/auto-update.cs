#!/usr/bin/env dotnet

// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:include ../utils/MarkdownTable.cs

using Khaos.Utils;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;

const string versionsPath = "runtimes/utils/sdl.env";
const string nuspecPath = "runtimes/package.nuspec";
const string readmePath = "readme.md";
const string runtimesReadmePath = "runtimes/readme.md";

Dictionary<string, Library> libraries = new()
{
    ["SDL3"] = new Library("SDL", "SDL3"),
    ["SDL3_IMAGE"] = new Library("SDL_image", "SDL_image"),
    ["SDL3_TTF"] = new Library("SDL_ttf", "SDL_ttf"),
    ["SDL3_MIXER"] = new Library("SDL_mixer", "SDL_mixer")
};

string? missingFile = Array.Find([versionsPath, nuspecPath, readmePath, runtimesReadmePath], f => !File.Exists(f));
if (!string.IsNullOrEmpty(missingFile))
{
    Console.Error.WriteLine($"Cannot find '{missingFile}'");
    return 1;
}

SdlVersions versions = await SdlVersions.ParseAsync(versionsPath);
RuntimePackage package = await RuntimePackage.ParseAsync(nuspecPath);
BundledVersionsTable bundledTable = await BundledVersionsTable.ParseAsync(runtimesReadmePath);
CompatibilityTable compatibilityTable = await CompatibilityTable.ParseAsync(readmePath);

using HttpClient http = CreateGitHubClient();

List<UpdatedRelease> updates = [];
Dictionary<string, Version> bundled = [];

foreach ((string key, Library library) in libraries)
{
    Version current = versions.GetVersion(key);
    bundled[library.DisplayName] = current;

    Version? latest = await GetLatestStableVersionAsync(library.Repository);

    if (latest is null)
    {
        Console.WriteLine($"{key}: no stable release found on libsdl-org/{library.Repository}.");
        continue;
    }

    if (latest <= current)
    {
        Console.WriteLine($"{key}: up to date ({current}).");
        continue;
    }

    Console.WriteLine($"{key}: {current} -> {latest}");

    versions.SetVersion(key, latest);
    bundled[library.DisplayName] = latest;
    updates.Add(new UpdatedRelease(key, library.Repository, current, latest));
}

if (updates.Count == 0)
{
    Console.WriteLine("All SDL libraries are up to date.");

    SetOutput("updated", "false");
    return 0;
}

Version previousRuntime = package.Version;
package.Bump(updates);

Version nextRuntime = package.Version;
Console.WriteLine($"Runtimes: {previousRuntime} -> {nextRuntime}");

bundledTable.Update(bundled);
compatibilityTable.SetSource(nextRuntime, bundled);

await versions.SaveAsync();
await package.SaveAsync();
await bundledTable.SaveAsync();
await compatibilityTable.SaveAsync();

string body = BuildTable(updates, "Newer stable SDL releases are available. This updates the versions used to build the native runtimes in `runtimes/utils/sdl.env`.", withLinks: true);
await File.WriteAllTextAsync("pr-body.md", $"{body}{Environment.NewLine}{BuildRuntimeNotice(previousRuntime, nextRuntime)}");

SetOutput("updated", "true");
AppendStepSummary(BuildTable(updates, "### SDL updates", withLinks: false));

Console.WriteLine($"Updated {updates.Count} SDL {(updates.Count == 1 ? "library" : "libraries")} in '{versionsPath}'.");
return 0;

static HttpClient CreateGitHubClient()
{
    HttpClient http = new()
    {
        BaseAddress = new Uri("https://api.github.com/"),
        Timeout = TimeSpan.FromSeconds(30)
    };

    string? token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

    if (!string.IsNullOrEmpty(token))
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
    http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("khaos-sdl-updater", "1.0"));
    http.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2026-03-10");

    return http;
}

async Task<Version?> GetLatestStableVersionAsync(string repository)
{
    using HttpResponseMessage response = await http.GetAsync($"repos/libsdl-org/{repository}/releases?per_page=100");

    if (!response.IsSuccessStatusCode)
    {
        string reason = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"GitHub returned {(int)response.StatusCode} for libsdl-org/{repository}: {reason}");
    }

    Release[]? releases = await response.Content.ReadFromJsonAsync(GithubContext.Default.ReleaseArray);
    Version? latest = null;

    foreach (Release release in releases ?? [])
    {
        if (release.Draft || release.Prerelease)
            continue;

        if (!TryParseReleaseVersion(release.Tag, out Version? version))
            continue;

        if (latest is null || version > latest)
            latest = version;
    }

    return latest;
}

static bool TryParseReleaseVersion(string? tag, out Version? version)
{
    const string prefix = "release-";
    version = null;

    if (tag?.StartsWith(prefix, StringComparison.Ordinal) != true)
        return false;

    return Version.TryParse(tag[prefix.Length..], out version);
}

static string BuildRuntimeNotice(Version previous, Version next)
{
    StringBuilder builder = new();

    builder.AppendLine(CultureInfo.InvariantCulture, $"`KappaDuck.Khaos.Runtimes` is bumped from `{previous}` to `{next}`, and the SDL tables in `readme.md` and `runtimes/readme.md` are updated.");
    builder.AppendLine();
    builder.AppendLine("> [!IMPORTANT]");
    builder.AppendLine("> The bump level follows the SDL version change. If a patch release adds native API, change the bump to a minor version before merging.");

    return builder.ToString();
}

static string BuildTable(IEnumerable<UpdatedRelease> updates, string heading, bool withLinks)
{
    StringBuilder builder = new();

    builder.AppendLine(heading);
    builder.AppendLine();
    builder.AppendLine("| Library | Current | Latest |");
    builder.AppendLine("| :---: | :---: | :---: |");

    foreach (UpdatedRelease update in updates)
    {
        string latest = withLinks
            ? $"[`{update.Latest}`](https://github.com/libsdl-org/{update.Repository}/releases/tag/release-{update.Latest})"
            : update.Latest.ToString();

        builder.AppendLine(CultureInfo.InvariantCulture, $"| `{update.Key}` | `{update.Current}` | {latest} |");
    }

    return builder.ToString();
}

static void SetOutput(string name, string value)
{
    string? file = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");

    if (!string.IsNullOrEmpty(file))
        File.AppendAllText(file, $"{name}={value}\n");
}

static void AppendStepSummary(string content)
{
    string? file = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");

    if (!string.IsNullOrEmpty(file))
        File.AppendAllText(file, content);
}

internal sealed record Release
{
    [JsonPropertyName("tag_name")]
    public string? Tag { get; init; }

    public bool Draft { get; init; }

    public bool Prerelease { get; init; }
}

internal sealed record Library(string Repository, string DisplayName);

internal sealed record UpdatedRelease(string Key, string Repository, Version Current, Version Latest);

internal sealed class SdlVersions
{
    private readonly string[] _lines;
    private readonly string _path;
    private readonly Dictionary<string, int> _entries = [];

    private SdlVersions(string[] lines, string path)
    {
        _lines = lines;
        _path = path;

        for (int i = 0; i < lines.Length; i++)
        {
            ReadOnlySpan<char> line = lines[i].AsSpan().Trim();
            int separator = line.IndexOf('=');

            if (separator <= 0 || line.StartsWith('#'))
                continue;

            _entries[line[..separator].Trim().ToString()] = i;
        }
    }

    public static async Task<SdlVersions> ParseAsync(string path)
        => new(await File.ReadAllLinesAsync(path), path);

    public Version GetVersion(string key)
    {
        if (!_entries.TryGetValue(key, out int index))
            throw new FormatException($"'{_path}' has no {key} entry.");

        ReadOnlySpan<char> line = _lines[index];
        ReadOnlySpan<char> text = line[(line.IndexOf('=') + 1)..].Trim();

        if (!Version.TryParse(text, out Version? version))
            throw new FormatException($"'{text}' is not a valid {key} version in '{_path}'.");

        return version;
    }

    public void SetVersion(string key, Version version)
    {
        if (!_entries.TryGetValue(key, out int index))
            throw new FormatException($"'{_path}' has no {key} entry.");

        _lines[index] = $"{key}={version}";
    }

    public Task SaveAsync() => File.WriteAllTextAsync(_path, string.Join('\n', _lines) + '\n');
}

internal sealed class RuntimePackage
{
    private const string OpenTag = "<version>";
    private const string CloseTag = "</version>";

    private readonly string _path;
    private readonly int _versionStart;
    private string _content;
    private int _versionLength;

    private RuntimePackage(string content, string path)
    {
        int open = content.IndexOf(OpenTag, StringComparison.Ordinal);
        int close = open < 0 ? -1 : content.IndexOf(CloseTag, open, StringComparison.Ordinal);

        if (close < 0)
            throw new FormatException($"Cannot find the package version in '{path}'.");

        _content = content;
        _path = path;
        _versionStart = open + OpenTag.Length;
        _versionLength = close - _versionStart;

        ReadOnlySpan<char> text = content.AsSpan(_versionStart, _versionLength).Trim();

        if (!Version.TryParse(text, out Version? version) || version.Build < 0)
            throw new FormatException($"'{text}' in '{path}' is not a major.minor.patch version.");

        Version = version;
    }

    public Version Version { get; private set; }

    public static async Task<RuntimePackage> ParseAsync(string path)
        => new(await File.ReadAllTextAsync(path), path);

    public void Bump(List<UpdatedRelease> updates)
    {
        bool major = updates.Exists(static u => u.Latest.Major != u.Current.Major);
        bool minor = updates.Exists(static u => u.Latest.Minor != u.Current.Minor);

        Version next = (major, minor) switch
        {
            (true, _) => new Version(Version.Major + 1, 0, 0),
            (false, true) => new Version(Version.Major, Version.Minor + 1, 0),
            (false, false) => new Version(Version.Major, Version.Minor, Version.Build + 1),
        };

        string text = next.ToString();

        _content = string.Concat(_content.AsSpan(0, _versionStart), text, _content.AsSpan(_versionStart + _versionLength));
        _versionLength = text.Length;
        Version = next;
    }

    public Task SaveAsync() => File.WriteAllTextAsync(_path, _content);
}

internal abstract class ReadmeTable
{
    private readonly string[] _lines;
    private readonly string _newLine;
    private readonly int _start;
    private readonly int _length;

    protected ReadmeTable(string content, string path, string heading)
    {
        _newLine = content.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        string[] lines = content.Split(_newLine);

        int headingIndex = Array.FindIndex(lines, l => l.AsSpan().Trim().SequenceEqual(heading));
        int start = headingIndex < 0 ? -1 : Array.FindIndex(lines, headingIndex + 1, static l => l.AsSpan().TrimStart().StartsWith('|'));

        if (start < 0)
            throw new FormatException($"Cannot find the table under '{heading}' in '{path}'.");

        int end = start;

        while (end < lines.Length && lines[end].AsSpan().TrimStart().StartsWith('|'))
            end++;

        _lines = lines;
        _start = start;
        _length = end - start;

        Path = path;
        Table = MarkdownTable.Parse(lines.AsSpan(start..end));
    }

    protected string Path { get; }

    protected MarkdownTable Table { get; }

    public Task SaveAsync()
    {
        string[] lines = [.. _lines.AsSpan(.._start), .. Table.Render(), .. _lines.AsSpan((_start + _length)..)];
        return File.WriteAllTextAsync(Path, string.Join(_newLine, lines));
    }

    protected static string Code(Version version) => $"`{version}`";
}

internal sealed class BundledVersionsTable : ReadmeTable
{
    private BundledVersionsTable(string content, string path)
        : base(content, path, "## Bundled versions")
    {
    }

    public static async Task<BundledVersionsTable> ParseAsync(string path)
        => new(await File.ReadAllTextAsync(path), path);

    public void Update(IReadOnlyDictionary<string, Version> bundled)
    {
        for (int row = 0; row < Table.RowCount; row++)
        {
            if (bundled.TryGetValue(Table[row, 0], out Version? version))
                Table[row, 1] = Code(version);
        }
    }
}

internal sealed class CompatibilityTable : ReadmeTable
{
    private const string Source = "`source`";

    private CompatibilityTable(string content, string path)
        : base(content, path, "## SDL compatibility")
    {
    }

    public static async Task<CompatibilityTable> ParseAsync(string path)
        => new(await File.ReadAllTextAsync(path), path);

    public void SetSource(Version runtime, IReadOnlyDictionary<string, Version> bundled)
    {
        ReadOnlySpan<string> header = Table.Header;
        string[] row = new string[header.Length];

        for (int column = 0; column < header.Length; column++)
        {
            string name = header[column];

            row[column] = name switch
            {
                "Khaos" => Source,
                "Runtimes" => Code(runtime),
                _ when bundled.TryGetValue(name, out Version? version) => Code(version),
                _ => throw new FormatException($"Unknown column '{name}' in the SDL compatibility table of '{Path}'.")
            };
        }

        if (Table.RowCount > 0 && Table[0, 0] == Source)
            Table.Replace(0, row);
        else
            Table.Insert(0, row);
    }
}

[JsonSerializable(typeof(Release[]))]
internal sealed partial class GithubContext : JsonSerializerContext;
