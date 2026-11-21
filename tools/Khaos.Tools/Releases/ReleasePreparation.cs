// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Markdown;
using KappaDuck.Khaos.Tools.Native;

namespace KappaDuck.Khaos.Tools.Releases;

public static class ReleasePreparation
{
    private const string NativeOpenTag = "<KhaosNativeVersion>";
    private const string NativeCloseTag = "</KhaosNativeVersion>";

    public const string ProjectPath = "Directory.Build.props";
    public const string PackagesPath = "Directory.Packages.props";
    public const string ChangelogPath = "changelog.md";
    public const string ReadmePath = "readme.md";

    public static IReadOnlyList<string> Files { get; } = [ProjectPath, ChangelogPath, ReadmePath, PackagesPath];

    public static Version NextVersion(string? requested, Version current)
    {
        ArgumentNullException.ThrowIfNull(current);

        if (string.IsNullOrEmpty(requested))
            return new Version(current.Major, current.Minor + 1, 0);

        if (!Version.TryParse(requested, out Version? parsed) || parsed.Build < 0 || parsed.Revision >= 0)
            throw new FormatException($"'{requested}' is not a major.minor.patch version.");

        return parsed;
    }

    public static async Task<NextVersion> OpenNextAsync(string root, string? requested, int year)
    {
        VersionPrefix project = await VersionPrefix.ParseAsync(Path.Combine(root, ProjectPath));

        Version current = project.Version;
        Version next = NextVersion(requested, current);
        project.Raise(next);

        Changelog changelog = await Changelog.ParseAsync(Path.Combine(root, ChangelogPath));
        changelog.OpenNext(current, next, year);

        await changelog.SaveAsync();
        await project.SaveAsync();

        return new NextVersion(current, next);
    }

    public static async Task<PreparedRelease> PrepareAsync(string root, DateOnly today)
    {
        string packagesPath = Path.Combine(root, PackagesPath);
        Version native = ReadNativeVersion(await File.ReadAllLinesAsync(packagesPath), packagesPath);

        VersionPrefix project = await VersionPrefix.ParseAsync(Path.Combine(root, ProjectPath));

        Changelog changelog = await Changelog.ParseAsync(Path.Combine(root, ChangelogPath));
        changelog.Release(project.Version, today);

        CompatibilityTable table = await CompatibilityTable.ParseAsync(Path.Combine(root, ReadmePath));
        table.AddRelease(project.Version, native);

        IReadOnlyList<AnalyzerProject> analyzers = await AnalyzerProject.FindAsync(root);

        foreach (AnalyzerProject analyzer in analyzers)
            analyzer.Ship(project.Version);

        await changelog.SaveAsync();
        await table.SaveAsync();

        foreach (AnalyzerProject analyzer in analyzers)
            await analyzer.SaveAsync();

        return new PreparedRelease(project.Version, native, analyzers);
    }

    private static Version ReadNativeVersion(string[] lines, string path)
    {
        string? line = Array.Find(lines, static l => l.Contains(NativeOpenTag, StringComparison.Ordinal));
        string? text = string.IsNullOrEmpty(line) ? null : Between(line, NativeOpenTag, NativeCloseTag);

        if (string.IsNullOrEmpty(text) || !Version.TryParse(text, out Version? version))
            throw new FormatException($"Cannot read {NativeOpenTag} in '{path}'.");

        return version;
    }

    private static string? Between(string text, string start, string end)
    {
        int from = text.IndexOf(start, StringComparison.Ordinal);

        if (from < 0)
            return null;

        from += start.Length;
        int to = text.IndexOf(end, from, StringComparison.Ordinal);

        return to < 0 ? null : text[from..to].Trim();
    }
}
