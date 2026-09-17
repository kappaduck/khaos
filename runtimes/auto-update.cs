#!/usr/bin/env dotnet

// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:include ../utils/markdownTable.cs

using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using Utils;

const string versionsPath = "runtimes/utils/sdl.env";
const string nuspecPath = "runtimes/package.nuspec";
const string readmePath = "readme.md";
const string runtimesReadmePath = "runtimes/readme.md";

const string compatibilityHeading = "## SDL compatibility";
const string bundledVersionsHeading = "## Bundled versions";
const string unreleasedKhaos = "`source`";

Dictionary<string, Library> libraries = new()
{
    ["SDL3"] = new Library("SDL", "SDL3"),
    ["SDL3_IMAGE"] = new Library("SDL_image", "SDL_image"),
    ["SDL3_TTF"] = new Library("SDL_ttf", "SDL_ttf"),
    ["SDL3_MIXER"] = new Library("SDL_mixer", "SDL_mixer")
};

string[] requiredFiles = [versionsPath, nuspecPath, readmePath, runtimesReadmePath];

foreach (string file in requiredFiles)
{
    if (!File.Exists(file))
    {
        Console.Error.WriteLine($"Cannot find '{file}'");
        return 1;
    }
}

using HttpClient http = CreateGitHubClient();

string[] lines = await File.ReadAllLinesAsync(versionsPath);
List<UpdatedRelease> updates = [];
Dictionary<string, Version> bundled = [];

for (int i = 0; i < lines.Length; i++)
{
    string line = lines[i];
    int separator = line.IndexOf('=');

    if (separator <= 0 || line.TrimStart().StartsWith('#'))
        continue;

    string key = line[..separator].Trim();
    string version = line[(separator + 1)..].Trim();

    if (!libraries.TryGetValue(key, out Library? library))
        continue;

    if (!Version.TryParse(version, out Version? current))
    {
        Console.Error.WriteLine($"skipping '{key}': '{version}' is not a valid version.");
        continue;
    }

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

    lines[i] = $"{key}={latest}";
    bundled[library.DisplayName] = latest;
    updates.Add(new UpdatedRelease(key, library.Repository, current, latest));
}

if (updates.Count == 0)
{
    Console.WriteLine("All SDL libraries are up to date.");

    SetOutput("updated", "false");
    return 0;
}

(Version previousRuntime, Version nextRuntime) = await BumpRuntimeVersionAsync(nuspecPath, updates);
Console.WriteLine($"Runtimes: {previousRuntime} -> {nextRuntime}");

await File.WriteAllTextAsync(versionsPath, string.Join('\n', lines) + '\n');
await UpdateMarkdownTableAsync(runtimesReadmePath, bundledVersionsHeading, table => UpdateBundledVersions(table, bundled));
await UpdateMarkdownTableAsync(readmePath, compatibilityHeading, table => AddCompatibilityRow(table, nextRuntime, bundled));

string body = BuildTable(updates, "Newer stable SDL releases are available. This updates the versions used to build the native runtimes in `runtimes/utils/sdl.env`.", withLinks: true)
    + Environment.NewLine
    + BuildRuntimeNotice(previousRuntime, nextRuntime);

await File.WriteAllTextAsync("pr-body.md", body);

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

    if (tag is null || !tag.StartsWith(prefix, StringComparison.Ordinal))
        return false;

    return Version.TryParse(tag[prefix.Length..], out version);
}

static async Task<(Version Previous, Version Next)> BumpRuntimeVersionAsync(string path, List<UpdatedRelease> updates)
{
    const string openTag = "<version>";
    const string closeTag = "</version>";

    string nuspec = await File.ReadAllTextAsync(path);

    int start = nuspec.IndexOf(openTag, StringComparison.Ordinal);
    int end = start < 0 ? -1 : nuspec.IndexOf(closeTag, start, StringComparison.Ordinal);

    if (start < 0 || end < 0)
        throw new InvalidOperationException($"Cannot find the package version in '{path}'.");

    start += openTag.Length;
    string text = nuspec[start..end].Trim();

    if (!Version.TryParse(text, out Version? previous) || previous.Build < 0)
        throw new InvalidOperationException($"'{text}' in '{path}' is not a major.minor.patch version.");

    Version next = NextRuntimeVersion(previous, updates);

    await File.WriteAllTextAsync(path, nuspec[..start] + next + nuspec[end..]);
    return (previous, next);
}

static Version NextRuntimeVersion(Version previous, List<UpdatedRelease> updates)
{
    if (updates.Exists(u => u.Latest.Major != u.Current.Major))
        return new Version(previous.Major + 1, 0, 0);

    if (updates.Exists(u => u.Latest.Minor != u.Current.Minor))
        return new Version(previous.Major, previous.Minor + 1, 0);

    return new Version(previous.Major, previous.Minor, previous.Build + 1);
}

static void UpdateBundledVersions(MarkdownTable table, Dictionary<string, Version> bundled)
{
    foreach (string[] row in table.Rows)
    {
        if (bundled.TryGetValue(row[0], out Version? version))
            row[1] = Code(version.ToString());
    }
}

static void AddCompatibilityRow(MarkdownTable table, Version runtime, Dictionary<string, Version> bundled)
{
    List<string> cells = [];

    foreach (string column in table.Header)
    {
        string cell = column switch
        {
            "Khaos" => unreleasedKhaos,
            "Runtimes" => Code(runtime.ToString()),
            _ when bundled.TryGetValue(column, out Version? version) => Code(version.ToString()),
            _ => throw new InvalidOperationException($"Unknown column '{column}' in the SDL compatibility table.")
        };

        cells.Add(cell);
    }

    string[] row = [.. cells];

    if (table.Rows.Count > 0 && table.Rows[0][0] == unreleasedKhaos)
        table.Rows[0] = row;
    else
        table.Rows.Insert(0, row);
}

static async Task UpdateMarkdownTableAsync(string path, string heading, Action<MarkdownTable> update)
{
    string content = await File.ReadAllTextAsync(path);
    string newLine = content.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
    List<string> lines = [.. content.Split('\n').Select(line => line.TrimEnd('\r'))];

    int headingIndex = lines.FindIndex(line => line.Trim() == heading);
    int start = headingIndex < 0 ? -1 : lines.FindIndex(headingIndex + 1, line => line.TrimStart().StartsWith('|'));

    if (start < 0)
        throw new InvalidOperationException($"Cannot find the table under '{heading}' in '{path}'.");

    int end = start;

    while (end < lines.Count && lines[end].TrimStart().StartsWith('|'))
        end++;

    MarkdownTable table = MarkdownTable.Parse([.. lines.GetRange(start, end - start)]);
    update(table);

    lines.RemoveRange(start, end - start);
    lines.InsertRange(start, table.Render().ToArray());

    await File.WriteAllTextAsync(path, string.Join(newLine, lines));
}

static string Code(string value) => $"`{value}`";

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
            ? $"[{update.Latest}](https://github.com/libsdl-org/{update.Repository}/releases/tag/release-{update.Latest})"
            : update.Latest.ToString();

        builder.AppendLine(CultureInfo.InvariantCulture, $"| `{update.Key}` | `{update.Current}` | `{latest}` |");
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

[JsonSerializable(typeof(Release[]))]
internal sealed partial class GithubContext : JsonSerializerContext
{
}
