// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Text;

namespace KappaDuck.Khaos.Tools.Benchmarks;

public static class ResultsPage
{
    private const string ResultsFolder = "artifacts/benchmarks";
    private const string ReportSuffix = "-report-github.md";

    public const string PagePath = "docs/performance/index.md";

    public static async Task<bool> UpdateAsync(string root)
    {
        string folder = Path.Combine(root, ResultsFolder);
        string pagePath = Path.Combine(root, PagePath);
        string comparisonPath = Path.Combine(folder, "comparison.md");

        IReadOnlyList<BenchmarkReport> frames = await ReadReportsAsync(folder, $"*.Frames.*{ReportSuffix}");
        IReadOnlyList<BenchmarkReport> micro = await ReadReportsAsync(folder, $"*.Micro.*{ReportSuffix}");
        string? comparison = File.Exists(comparisonPath) ? await File.ReadAllTextAsync(comparisonPath) : null;

        string? page = AddResults(await File.ReadAllTextAsync(pagePath), comparison, frames, micro);

        if (page is null)
            return false;

        await File.WriteAllTextAsync(pagePath, page);
        return true;
    }

    private static async Task<IReadOnlyList<BenchmarkReport>> ReadReportsAsync(string folder, string pattern)
    {
        if (!Directory.Exists(folder))
            return [];

        string[] files = Directory.GetFiles(folder, pattern);
        Array.Sort(files, StringComparer.Ordinal);

        List<BenchmarkReport> reports = [];

        foreach (string file in files)
            reports.Add(new BenchmarkReport(ClassName(Path.GetFileName(file)), await File.ReadAllTextAsync(file)));

        return reports;
    }

    private static string ClassName(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);

        string name = fileName[..^ReportSuffix.Length];
        return name[(name.LastIndexOf('.', StringComparison.OrdinalIgnoreCase) + 1)..];
    }

    private static string? AddResults(string page, string? comparison, IReadOnlyList<BenchmarkReport> frames, IReadOnlyList<BenchmarkReport> micro)
    {
        ArgumentNullException.ThrowIfNull(frames);
        ArgumentNullException.ThrowIfNull(micro);

        if (frames.Count + micro.Count == 0)
            return null;

        StringBuilder builder = new(BeforeTheNote(page));

        if (comparison is not null)
            builder.Append('\n').Append(comparison);

        AddSection(builder, "One frame", frames);
        AddSection(builder, "One operation at a time", micro);

        return builder.ToString();
    }

    private static string BeforeTheNote(string page)
    {
        int start = 0;

        while (start < page.Length)
        {
            if (page.AsSpan(start).StartsWith("> [!NOTE]", StringComparison.Ordinal))
                return page[..start];

            int end = page.IndexOf('\n', start, StringComparison.Ordinal);

            if (end < 0)
                break;

            start = end + 1;
        }

        return page;
    }

    private static void AddSection(StringBuilder builder, string heading, IReadOnlyList<BenchmarkReport> reports)
    {
        if (reports.Count == 0)
            return;

        builder.Append("\n## ").Append(heading).Append('\n');

        foreach (BenchmarkReport report in reports)
            builder.Append("\n### ").Append(report.Name).Append("\n\n").Append(report.Content);
    }

    private sealed record BenchmarkReport(string Name, string Content);
}
