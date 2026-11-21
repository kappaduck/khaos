// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Globalization;

namespace KappaDuck.Khaos.Tools.Releases;

public sealed class AnalyzerProject
{
    public const string Folder = "analyzers";

    private const string ReleasePrefix = "## Release ";
    private const string RuleIdHeader = "Rule ID";
    private const string ShippedFile = "AnalyzerReleases.Shipped.md";
    private const string UnshippedFile = "AnalyzerReleases.Unshipped.md";

    private readonly string _shippedPath;
    private readonly string _unshippedPath;
    private string[] _shipped;
    private string[] _unshipped;

    private AnalyzerProject(string name, string directory, string[] shipped, string[] unshipped)
    {
        _shippedPath = Path.Combine(directory, ShippedFile);
        _unshippedPath = Path.Combine(directory, UnshippedFile);
        _shipped = shipped;
        _unshipped = unshipped;

        Name = name;
    }

    public string Name { get; }

    public IReadOnlyList<string> ShippedRules { get; private set; } = [];

    public static async Task<IReadOnlyList<AnalyzerProject>> FindAsync(string root)
    {
        string folder = Path.Combine(root, Folder);

        if (!Directory.Exists(folder))
            throw new DirectoryNotFoundException($"Cannot find the '{Folder}' folder.");

        List<AnalyzerProject> analyzers = [];

        foreach (string directory in Directory.GetDirectories(folder))
        {
            string shippedPath = Path.Combine(directory, ShippedFile);
            string unshippedPath = Path.Combine(directory, UnshippedFile);

            if (!File.Exists(shippedPath) || !File.Exists(unshippedPath))
                continue;

            string[] shipped = await File.ReadAllLinesAsync(shippedPath);
            string[] unshipped = await File.ReadAllLinesAsync(unshippedPath);

            analyzers.Add(new AnalyzerProject($"{Folder}/{Path.GetFileName(directory)}", directory, shipped, unshipped));
        }

        return analyzers;
    }

    public async Task SaveAsync()
    {
        if (ShippedRules.Count == 0)
            return;

        await File.WriteAllTextAsync(_shippedPath, Join(_shipped));
        await File.WriteAllTextAsync(_unshippedPath, Join(_unshipped));

        static string Join(string[] lines) => lines.Length == 0 ? string.Empty : string.Join('\n', lines) + '\n';
    }

    public void Ship(Version version)
    {
        string heading = string.Create(CultureInfo.InvariantCulture, $"{ReleasePrefix}{version}");

        if (Array.Exists(_shipped, l => l.AsSpan().TrimEnd().SequenceEqual(heading)))
            throw new InvalidOperationException($"'{Name}/{ShippedFile}' already has a section for {version}.");

        ReadOnlySpan<string> rules = TrimBlankLines(_unshipped);

        ShippedRules = ReadRuleIds(rules);

        if (ShippedRules.Count == 0)
            return;

        ReadOnlySpan<string> shipped = TrimBlankLines(_shipped);

        _shipped = shipped.IsEmpty
            ? [heading, string.Empty, .. rules]
            : [.. shipped, string.Empty, heading, string.Empty, .. rules];

        _unshipped = [];
    }

    private static ReadOnlySpan<string> TrimBlankLines(ReadOnlySpan<string> lines)
    {
        int start = 0;

        while (start < lines.Length && string.IsNullOrWhiteSpace(lines[start]))
            start++;

        int end = lines.Length;

        while (end > start && string.IsNullOrWhiteSpace(lines[end - 1]))
            end--;

        return lines[start..end];
    }

    private static string[] ReadRuleIds(ReadOnlySpan<string> lines)
    {
        List<string> ids = [];

        foreach (string line in lines)
        {
            if (IsRuleRow(line))
                ids.Add(ReadRuleId(line));
        }

        return [.. ids];
    }

    private static string ReadRuleId(ReadOnlySpan<char> line)
    {
        ReadOnlySpan<char> cells = line.Trim().TrimStart('|');
        int end = cells.IndexOf('|');

        return cells[..(end < 0 ? cells.Length : end)].Trim().ToString();
    }

    private static bool IsRuleRow(ReadOnlySpan<char> line)
    {
        if (!line.Contains('|'))
            return false;

        ReadOnlySpan<char> cells = line.Trim().TrimStart('|').TrimStart();
        return !cells.StartsWith(RuleIdHeader, StringComparison.Ordinal) && !cells.StartsWith('-') && !cells.StartsWith(':');
    }
}
