#!/usr/bin/env dotnet
// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:include markdownTable.cs

using System.Globalization;
using Utils;

const string projectPath = "src/Khaos/Khaos.csproj";
const string changelogPath = "changelog.md";
const string readmePath = "readme.md";
const string packagesPath = "Directory.Packages.props";

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
catch (ReleaseException exception)
{
    Console.Error.WriteLine($"error: {exception.Message}");
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
    TextFile project = await TextFile.LoadAsync(projectPath);
    TextFile changelog = await TextFile.LoadAsync(changelogPath);
    TextFile readme = await TextFile.LoadAsync(readmePath);
    TextFile packages = await TextFile.LoadAsync(packagesPath);

    Version version = ReadVersionPrefix(project);
    string today = DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    int unreleased = changelog.Lines.FindIndex(IsUnreleasedHeading);

    if (unreleased < 0)
        throw new ReleaseException($"'{changelogPath}' has no {Changelog.UnreleasedMarker} section.");

    if (changelog.Lines.Exists(line => IsVersionHeading(line, version)))
        throw new ReleaseException($"'{changelogPath}' already has a section for {version}.");

    if (IsSectionEmpty(changelog.Lines, unreleased))
        throw new ReleaseException($"The {Changelog.UnreleasedMarker} section of '{changelogPath}' is empty.");

    changelog.Lines[unreleased] = $"## {version} {Changelog.Separator} {today}";

    CompatibilityTable table = CompatibilityTable.Find(readme);
    string[] source = table.SourceRow();

    if (table.Table.Rows.Any(row => row[table.KhaosColumn] == Code(version)))
        throw new ReleaseException($"The SDL compatibility table already has a row for Khaos {version}.");

    Version runtime = ReadRuntimeVersion(packages);
    Version? sourceRuntime = ParseRuntime(source[table.RuntimesColumn]);

    if (sourceRuntime != runtime)
    {
        throw new ReleaseException(
            $"Khaos references KappaDuck.Khaos.Runtimes {runtime} in '{packagesPath}', but the `source` row of the SDL "
            + $"compatibility table ships {sourceRuntime?.ToString() ?? "an unknown version"}. Update one of them first.");
    }

    string[] released = [.. source];
    released[table.KhaosColumn] = Code(version);
    released[table.RuntimesColumn] = Code(runtime);
    table.Table.Rows.Insert(1, released);
    table.Save();

    await changelog.SaveAsync();
    await readme.SaveAsync();

    Console.WriteLine($"Prepared Khaos {version} ({today}):");
    Console.WriteLine($"  {changelogPath}: dated the {Changelog.UnreleasedMarker} section");
    Console.WriteLine($"  {readmePath}: added Khaos {version} with KappaDuck.Khaos.Runtimes {runtime} to the SDL compatibility table");
    return 0;
}

static async Task<int> PrepareNextAsync(string? requested)
{
    TextFile project = await TextFile.LoadAsync(projectPath);
    TextFile changelog = await TextFile.LoadAsync(changelogPath);

    Version current = ReadVersionPrefix(project);
    Version next = NextKhaosVersion(requested, current);

    if (next <= current)
        throw new ReleaseException($"The next version ({next}) must be greater than the current one ({current}).");

    if (changelog.Lines.Exists(IsUnreleasedHeading))
        throw new ReleaseException($"'{changelogPath}' already has an {Changelog.UnreleasedMarker} section.");

    if (!changelog.Lines.Exists(line => IsVersionHeading(line, current)))
        throw new ReleaseException($"'{changelogPath}' has no section for {current}. Prepare and publish the release first.");

    int firstSection = changelog.Lines.FindIndex(line => line.StartsWith("## ", StringComparison.Ordinal));
    string placeholder = string.Create(CultureInfo.InvariantCulture, $"## {next} {Changelog.Separator} {DateTime.Now.Year}-xx-xx {Changelog.UnreleasedMarker}");

    changelog.Lines.InsertRange(firstSection < 0 ? changelog.Lines.Count : firstSection, [placeholder, string.Empty]);

    int prefix = project.Lines.FindIndex(line => line.Contains(VersionPrefix.OpenTag, StringComparison.Ordinal));
    project.Lines[prefix] = project.Lines[prefix].Replace(
        VersionPrefix.OpenTag + current + VersionPrefix.CloseTag,
        VersionPrefix.OpenTag + next + VersionPrefix.CloseTag,
        StringComparison.Ordinal);

    await changelog.SaveAsync();
    await project.SaveAsync();

    Console.WriteLine($"Prepared the next version: {current} -> {next}");
    Console.WriteLine($"  {projectPath}: VersionPrefix is {next}");
    Console.WriteLine($"  {changelogPath}: opened the {Changelog.UnreleasedMarker} section");
    return 0;
}

static Version NextKhaosVersion(string? requested, Version current)
{
    if (requested is null)
        return new Version(current.Major, current.Minor + 1, 0);

    if (!Version.TryParse(requested, out Version? parsed) || parsed.Build < 0 || parsed.Revision >= 0)
        throw new ReleaseException($"'{requested}' is not a major.minor.patch version.");

    return parsed;
}

static Version ReadVersionPrefix(TextFile project)
{
    string? line = project.Lines.Find(candidate => candidate.Contains(VersionPrefix.OpenTag, StringComparison.Ordinal));
    string? text = line is null ? null : Between(line, VersionPrefix.OpenTag, VersionPrefix.CloseTag);

    if (text is null || !Version.TryParse(text, out Version? version) || version.Build < 0 || version.Revision >= 0)
        throw new ReleaseException($"Cannot read a major.minor.patch VersionPrefix in '{projectPath}'.");

    return version;
}

static Version ReadRuntimeVersion(TextFile packages)
{
    const string include = "Include=\"KappaDuck.Khaos.Runtimes\"";

    string? line = packages.Lines.Find(candidate => candidate.Contains(include, StringComparison.Ordinal));
    string? text = line is null ? null : Between(line, "Version=\"", "\"");

    if (text is null || !Version.TryParse(text, out Version? version))
        throw new ReleaseException($"Cannot read the KappaDuck.Khaos.Runtimes version in '{packagesPath}'.");

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

static Version? ParseRuntime(string cell)
    => Version.TryParse(cell.Trim('`'), out Version? version) ? version : null;

static bool IsUnreleasedHeading(string line)
    => line.StartsWith("## ", StringComparison.Ordinal) && line.TrimEnd().EndsWith(Changelog.UnreleasedMarker, StringComparison.Ordinal);

static bool IsVersionHeading(string line, Version version)
    => line.StartsWith($"## {version} ", StringComparison.Ordinal);

static bool IsSectionEmpty(List<string> lines, int heading)
{
    for (int i = heading + 1; i < lines.Count; i++)
    {
        if (lines[i].StartsWith("## ", StringComparison.Ordinal))
            return true;

        if (!string.IsNullOrWhiteSpace(lines[i]))
            return false;
    }

    return true;
}

static string Code(Version version) => $"`{version}`";

internal static class Changelog
{
    public const string UnreleasedMarker = "[Unreleased]";
    public const string Separator = "&#8212;";
}

internal static class VersionPrefix
{
    public const string OpenTag = "<VersionPrefix>";
    public const string CloseTag = "</VersionPrefix>";
}

internal sealed class ReleaseException(string message) : Exception(message);

internal sealed class TextFile
{
    private TextFile(string path, string newLine, List<string> lines)
    {
        Path = path;
        NewLine = newLine;
        Lines = lines;
    }

    public string Path { get; }

    public string NewLine { get; }

    public List<string> Lines { get; }

    public static async Task<TextFile> LoadAsync(string path)
    {
        if (!File.Exists(path))
            throw new ReleaseException($"Cannot find '{path}'. Run the script from the repository root.");

        string content = await File.ReadAllTextAsync(path);
        string newLine = content.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";

        return new TextFile(path, newLine, [.. content.Split('\n').Select(line => line.TrimEnd('\r'))]);
    }

    public Task SaveAsync() => File.WriteAllTextAsync(Path, string.Join(NewLine, Lines));
}

internal sealed class CompatibilityTable
{
    private const string Heading = "## SDL compatibility";
    private const string Source = "`source`";

    private readonly TextFile _file;
    private readonly int _start;
    private readonly int _length;

    private CompatibilityTable(TextFile file, int start, int length, MarkdownTable table)
    {
        _file = file;
        _start = start;
        _length = length;
        Table = table;
        KhaosColumn = IndexOfColumn(table.Header, "Khaos");
        RuntimesColumn = IndexOfColumn(table.Header, "Runtimes");

        if (KhaosColumn < 0 || RuntimesColumn < 0)
            throw new ReleaseException($"The SDL compatibility table in '{file.Path}' needs a Khaos and a Runtimes column.");
    }

    public MarkdownTable Table { get; }

    public int KhaosColumn { get; }

    public int RuntimesColumn { get; }

    public static CompatibilityTable Find(TextFile file)
    {
        List<string> lines = file.Lines;

        int heading = lines.FindIndex(line => line.Trim() == Heading);
        int start = heading < 0 ? -1 : lines.FindIndex(heading + 1, line => line.TrimStart().StartsWith('|'));

        if (start < 0)
            throw new ReleaseException($"Cannot find the table under '{Heading}' in '{file.Path}'.");

        int end = start;

        while (end < lines.Count && lines[end].TrimStart().StartsWith('|'))
            end++;

        return new CompatibilityTable(file, start, end - start, MarkdownTable.Parse([.. lines.GetRange(start, end - start)]));
    }

    public string[] SourceRow()
    {
        if (Table.Rows.Count == 0 || Table.Rows[0][KhaosColumn] != Source)
            throw new ReleaseException($"The first row of the SDL compatibility table in '{_file.Path}' must be the {Source} row.");

        return Table.Rows[0];
    }

    private static int IndexOfColumn(IReadOnlyList<string> header, string name)
    {
        for (int column = 0; column < header.Count; column++)
        {
            if (header[column] == name)
                return column;
        }

        return -1;
    }

    public void Save()
    {
        _file.Lines.RemoveRange(_start, _length);
        _file.Lines.InsertRange(_start, Table.Render().ToArray());
    }
}
