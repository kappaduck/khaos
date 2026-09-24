#!/usr/bin/env dotnet

// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:include MarkdownTable.cs

using Khaos.Utils;
using System.Globalization;

const string projectPath = "src/Khaos/Khaos.csproj";
const string changelogPath = "changelog.md";
const string readmePath = "readme.md";
const string packagesPath = "Directory.Packages.props";

string? missingFile = Array.Find([projectPath, changelogPath, readmePath, packagesPath], static f => !File.Exists(f));
if (!string.IsNullOrEmpty(missingFile))
{
    Console.Error.WriteLine($"error: Cannot find '{missingFile}'. Run the script from the repository root.");
    return 1;
}

try
{
    return args switch
    {
        [] => await PrepareReleaseAsync(),
        ["--next"] => await PrepareNextAsync(null),
        ["--next", string next] => await PrepareNextAsync(next),
        _ => Usage()
    };
}
catch (Exception ex)
{
    Console.Error.WriteLine($"error: {ex.Message}");
    Console.Error.WriteLine("No file was modified.");
    return 1;
}

static int Usage()
{
    Console.Error.WriteLine("usage: dotnet utils/prepare-release.cs [--next [x.y.z]]");
    return 1;
}

static async Task<int> PrepareReleaseAsync()
{
    string[] packages = await File.ReadAllLinesAsync(packagesPath);

    Version runtime = ReadRuntimeVersion(packages);
    DateOnly today = DateOnly.FromDateTime(DateTime.Now);

    Project project = await Project.ParseAsync(projectPath);

    Changelog changelog = await Changelog.ParseAsync(changelogPath);
    changelog.Release(project.Version, today);

    CompatibilityTable table = await CompatibilityTable.ParseAsync(readmePath);
    table.AddRelease(project.Version, runtime);

    await changelog.SaveAsync();
    await table.SaveAsync();

    Console.WriteLine($"Prepared Khaos {project.Version} ({today:yyyy-MM-dd}):");
    Console.WriteLine($"  {changelogPath}: dated the [Unreleased] section");
    Console.WriteLine($"  {readmePath}: added Khaos {project.Version} with KappaDuck.Khaos.Runtimes {runtime} to the SDL compatibility table");
    return 0;
}

static async Task<int> PrepareNextAsync(string? requested)
{
    Project project = await Project.ParseAsync(projectPath);

    Version current = project.Version;
    Version next = NextVersion(requested, current);
    project.SetVersion(next);

    Changelog changelog = await Changelog.ParseAsync(changelogPath);
    changelog.OpenNext(current, next);

    await changelog.SaveAsync();
    await project.SaveAsync();

    Console.WriteLine($"Prepared the next version: {current} -> {next}");
    Console.WriteLine($"  {projectPath}: VersionPrefix is {next}");
    Console.WriteLine($"  {changelogPath}: opened the [Unreleased] section");
    return 0;
}

static Version NextVersion(string? requested, Version current)
{
    if (string.IsNullOrEmpty(requested))
        return new Version(current.Major, current.Minor + 1, 0);

    if (!Version.TryParse(requested, out Version? parsed) || parsed.Build < 0 || parsed.Revision >= 0)
        throw new FormatException($"'{requested}' is not a major.minor.patch version.");

    return parsed;
}

static Version ReadRuntimeVersion(string[] packages)
{
    const string include = "Include=\"KappaDuck.Khaos.Runtimes\"";

    string? line = Array.Find(packages, c => c.Contains(include, StringComparison.Ordinal));
    string? text = string.IsNullOrEmpty(line) ? null : Between(line, "Version=\"", "\"");

    if (string.IsNullOrEmpty(text) || !Version.TryParse(text, out Version? version))
        throw new FormatException($"Cannot read the KappaDuck.Khaos.Runtimes version in '{packagesPath}'.");

    return version;
}

static string? Between(string text, string start, string end)
{
    int from = text.IndexOf(start, StringComparison.Ordinal);

    if (from < 0)
        return null;

    from += start.Length;
    int to = text.IndexOf(end, from, StringComparison.Ordinal);

    return to < 0 ? null : text[from..to].Trim();
}

internal sealed class Changelog
{
    private const string UnreleasedMarker = "[Unreleased]";
    private const string Separator = "&#8212;";
    private const string SectionPrefix = "## ";

    private readonly string _path;
    private string[] _lines;

    private Changelog(string path, string[] lines)
    {
        _lines = lines;
        _path = path;
    }

    internal static async Task<Changelog> ParseAsync(string path)
    {
        string[] lines = await File.ReadAllLinesAsync(path);
        return new Changelog(path, lines);
    }

    internal void Release(Version version, DateOnly date)
    {
        int unreleased = Array.FindIndex(_lines, static l => IsUnreleasedHeading(l));

        if (unreleased < 0)
            throw new InvalidOperationException($"'{_path}' has no {UnreleasedMarker} section.");

        if (HasSection(version))
            throw new InvalidOperationException($"'{_path}' already has a section for {version}.");

        if (IsSectionEmpty(unreleased))
            throw new InvalidOperationException($"The {UnreleasedMarker} section of '{_path}' is empty.");

        _lines[unreleased] = string.Create(CultureInfo.InvariantCulture, $"{SectionPrefix}{version} {Separator} {date:yyyy-MM-dd}");
    }

    internal void OpenNext(Version current, Version next)
    {
        if (Array.Exists(_lines, static l => IsUnreleasedHeading(l)))
            throw new InvalidOperationException($"'{_path}' already has an {UnreleasedMarker} section.");

        if (!HasSection(current))
            throw new InvalidOperationException($"'{_path}' has no section for {current}. Prepare and publish the release first.");

        int firstSection = Array.FindIndex(_lines, static l => l.StartsWith(SectionPrefix, StringComparison.Ordinal));
        string placeholder = string.Create(CultureInfo.InvariantCulture, $"{SectionPrefix}{next} {Separator} {DateTime.Now.Year}-xx-xx {UnreleasedMarker}");

        _lines = [.. _lines.AsSpan(..firstSection), placeholder, string.Empty, .. _lines.AsSpan(firstSection..)];
    }

    internal Task SaveAsync() => File.WriteAllTextAsync(_path, string.Join('\n', _lines) + '\n');


    private bool HasSection(Version version) => Array.Exists(_lines, l => IsVersionHeading(l, version));

    private bool IsSectionEmpty(int heading)
    {
        for (int i = heading + 1; i < _lines.Length; i++)
        {
            if (_lines[i].StartsWith(SectionPrefix, StringComparison.Ordinal))
                return true;

            if (!string.IsNullOrWhiteSpace(_lines[i]))
                return false;
        }

        return true;
    }

    private static bool IsUnreleasedHeading(ReadOnlySpan<char> line)
        => line.StartsWith(SectionPrefix, StringComparison.Ordinal) && line.TrimEnd().EndsWith(UnreleasedMarker, StringComparison.Ordinal);

    private static bool IsVersionHeading(ReadOnlySpan<char> line, Version version)
        => line.StartsWith($"{SectionPrefix}{version} ", StringComparison.Ordinal) && !IsUnreleasedHeading(line);
}

internal sealed class Project
{
    private const string OpenTag = "<VersionPrefix>";
    private const string CloseTag = "</VersionPrefix>";

    private readonly string _path;
    private string _content;

    private Project(string path, string content, Version version)
    {
        _path = path;
        _content = content;

        Version = version;
    }

    public Version Version { get; private set; }

    internal static async Task<Project> ParseAsync(string path)
    {
        string content = await File.ReadAllTextAsync(path);

        (int start, int end) = GetVersionIndexes(content);
        ReadOnlySpan<char> versionTag = content.AsSpan(start, end).Trim();

        if (!Version.TryParse(versionTag, out Version? version) || version.Build < 0 || version.Revision >= 0)
            throw new FormatException($"'{version}' is not a major.minor.patch VersionPrefix in '{path}'.");

        return new Project(path, content, version);
    }

    internal void SetVersion(Version version)
    {
        if (version <= Version)
            throw new ArgumentException($"The next version ({version}) must be greater than the current one ({Version}).");

        string text = version.ToString();

        (int start, int end) = GetVersionIndexes(_content);

        _content = string.Concat(_content.AsSpan(0, start), text, _content.AsSpan(start + end));
        Version = version;
    }

    internal Task SaveAsync() => File.WriteAllTextAsync(_path, _content);

    private static (int Start, int End) GetVersionIndexes(string content)
    {
        int open = content.IndexOf(OpenTag, StringComparison.Ordinal);
        int close = open < 0 ? -1 : content.IndexOf(CloseTag, open, StringComparison.Ordinal);

        if (close < 0)
            throw new FormatException($"Cannot find a {OpenTag} element in '.csproj'.");

        int start = open + OpenTag.Length;
        return (start, close - start);
    }
}

internal sealed class CompatibilityTable
{
    private const string Heading = "## SDL compatibility";
    private const string Source = "`source`";

    private readonly string[] _lines;
    private readonly string _path;
    private readonly int _start;
    private readonly int _length;
    private readonly MarkdownTable _table;
    private readonly int _khaosColumn;
    private readonly int _runtimesColumn;

    internal CompatibilityTable(string path, string[] lines, int start, int length, MarkdownTable table, int khaosColumn, int runtimesColumn)
    {
        _lines = lines;
        _path = path;
        _start = start;
        _length = length;
        _table = table;
        _khaosColumn = khaosColumn;
        _runtimesColumn = runtimesColumn;
    }

    internal static async Task<CompatibilityTable> ParseAsync(string path)
    {
        string[] lines = await File.ReadAllLinesAsync(path);

        int heading = Array.FindIndex(lines, static l => l.AsSpan().Trim().SequenceEqual(Heading));
        int start = heading < 0 ? -1 : Array.FindIndex(lines, heading + 1, static l => l.AsSpan().TrimStart().StartsWith('|'));

        if (start < 0)
            throw new FormatException($"Cannot find the table under '{Heading}' in '{path}'.");

        int end = start;

        while (end < lines.Length && lines[end].AsSpan().TrimStart().StartsWith('|'))
            end++;

        MarkdownTable table = MarkdownTable.Parse(lines.AsSpan(start..end));

        int khaosColumn = table.IndexOfColumn("Khaos");
        int runtimesColumn = table.IndexOfColumn("Runtimes");

        if (khaosColumn < 0 || runtimesColumn < 0)
            throw new FormatException($"The SDL compatibility table in '{path}' needs a Khaos and a Runtimes column.");

        if (table.RowCount == 0 || table[0, khaosColumn] != Source)
            throw new FormatException($"The first row of the SDL compatibility table in '{path}' must be the {Source} row.");

        return new CompatibilityTable(path, lines, start, end - start, table, khaosColumn, runtimesColumn);
    }

    public void AddRelease(Version khaos, Version runtime)
    {
        string khaosCell = Code(khaos);

        if (_table.IndexOfRow(_khaosColumn, khaosCell) >= 0)
            throw new InvalidOperationException($"The SDL compatibility table already has a row for Khaos {khaos}.");

        Version? sourceRuntime = ParseVersion(_table[0, _runtimesColumn]);

        if (sourceRuntime != runtime)
            throw new InvalidOperationException($"Khaos references KappaDuck.Khaos.Runtimes {runtime}, but the {Source} row of the SDL compatibility table ships {sourceRuntime?.ToString() ?? "an unknown version"}. Update one of them first.");

        string[] release = _table[0].ToArray();
        release[_khaosColumn] = khaosCell;
        release[_runtimesColumn] = Code(runtime);

        _table.Insert(1, release);
    }

    public Task SaveAsync()
    {
        string[] lines = [.. _lines.AsSpan(.._start), .. _table.Render(), .. _lines.AsSpan((_start + _length)..)];
        return File.WriteAllTextAsync(_path, string.Join('\n', lines) + '\n');
    }

    private static string Code(Version version) => $"`{version}`";

    private static Version? ParseVersion(string cell)
        => Version.TryParse(cell.AsSpan().Trim('`'), out Version? version) ? version : null;
}
