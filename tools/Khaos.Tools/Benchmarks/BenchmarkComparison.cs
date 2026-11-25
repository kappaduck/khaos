// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Markdown;
using Perfolizer.Mathematics.Common;
using Perfolizer.Mathematics.SignificanceTesting;
using Perfolizer.Mathematics.SignificanceTesting.MannWhitney;
using Pragmastat;
using System.Globalization;
using System.Text.Json;
using Threshold = Perfolizer.Metrology.Threshold;

namespace KappaDuck.Khaos.Tools.Benchmarks;

public static class BenchmarkComparison
{
    private const string Prefix = "KappaDuck.Khaos.Benchmarks.";

    public static int Write(TextWriter output, ComparisonOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(output);

        Dictionary<string, BenchmarkResult> before = Load(options.Base);
        Dictionary<string, BenchmarkResult> after = Load(options.Diff);

        List<BenchmarkChange> changes = Compare(before, after, Threshold.Parse(options.Threshold));
        int allocationsUp = changes.Count(static change => change.AllocationWentUp);

        output.WriteLine($"## Compared with {options.BaseName}");
        output.WriteLine();

        if (changes.Count == 0)
        {
            output.WriteLine($"No change beyond {options.Threshold} since {options.BaseName}.");
        }
        else
        {
            output.WriteLine($"Both versions measured on the same runner, one after the other. Time changes under {options.Threshold} are left out.");
            output.WriteLine();

            foreach (string line in Table(options, changes))
                output.WriteLine(line);
        }

        if (allocationsUp > 0)
        {
            output.WriteLine();
            output.WriteLine("> [!WARNING]");
            output.WriteLine($"> The allocations went up in {allocationsUp} {(allocationsUp == 1 ? "benchmark" : "benchmarks")}.");
        }

        string[] added = [.. after.Keys.Where(key => !before.ContainsKey(key)).Select(key => $"`{after[key].Label}`").Order(StringComparer.Ordinal)];
        string[] gone = [.. before.Keys.Where(key => !after.ContainsKey(key)).Select(key => $"`{before[key].Label}`").Order(StringComparer.Ordinal)];

        if (added.Length > 0)
        {
            output.WriteLine();
            output.WriteLine($"New in {options.DiffName}: {string.Join(", ", added)}.");
        }

        if (gone.Length > 0)
        {
            output.WriteLine();
            output.WriteLine($"Gone since {options.BaseName}: {string.Join(", ", gone)}.");
        }

        return allocationsUp;
    }

    private static Dictionary<string, BenchmarkResult> Load(string path)
    {
        Dictionary<string, BenchmarkResult> results = [];
        IEnumerable<string> files = File.Exists(path) ? [path] : Directory.EnumerateFiles(path, "*-report-full*.json");

        foreach (string file in files)
        {
            using FileStream stream = File.OpenRead(file);
            BenchmarkReport? report = JsonSerializer.Deserialize(stream, BenchmarkJsonContext.Default.BenchmarkReport);

            foreach (BenchmarkEntry benchmark in report?.Benchmarks ?? [])
            {
                BenchmarkResult result = BenchmarkResult.From(benchmark, Prefix);
                results[result.Key] = result;
            }
        }

        return results;
    }

    private static List<BenchmarkChange> Compare(Dictionary<string, BenchmarkResult> before, Dictionary<string, BenchmarkResult> after, Threshold threshold)
    {
        List<BenchmarkChange> changes = [];

        foreach ((string key, BenchmarkResult current) in after)
        {
            if (!before.TryGetValue(key, out BenchmarkResult? baseline))
                continue;

            string verdict = Verdict(baseline, current, threshold);

            if (verdict is "Slower" or "Faster" || baseline.Allocated != current.Allocated)
                changes.Add(new BenchmarkChange(baseline, current, verdict));
        }

        changes.Sort(static (x, y) =>
        {
            int byRatio = y.Ratio.CompareTo(x.Ratio);
            return byRatio != 0 ? byRatio : string.CompareOrdinal(x.Current.Name, y.Current.Name);
        });

        return changes;
    }

    private static string Verdict(BenchmarkResult baseline, BenchmarkResult current, Threshold threshold)
    {
        if (baseline.Values.SequenceEqual(current.Values))
            return "Same";

        if (baseline.Values.Count < 2 || current.Values.Count < 2)
            return "?";

#pragma warning disable CS0618 // SimpleEquivalenceTest is obsolete in Perfolizer 0.7, and still the test this comparison relies on.
        SimpleEquivalenceTest test = new(MannWhitneyTest.Instance);
        ComparisonResult result = test.Perform(new Sample(current.Values), new Sample(baseline.Values), threshold, SignificanceLevel.P1E5);
#pragma warning restore CS0618

        return result switch
        {
            ComparisonResult.Greater => "Slower",
            ComparisonResult.Lesser => "Faster",
            ComparisonResult.Indistinguishable => "Same",
            ComparisonResult.Unknown => "?",
            _ => "?",
        };
    }

    private static string[] Table(ComparisonOptions options, List<BenchmarkChange> changes)
    {
        bool showRuntime = changes.Exists(static change => change.Current.Job != BenchmarkResult.DefaultJob);

        string header = showRuntime
            ? $"| Benchmark | Runtime | {options.BaseName} | {options.DiffName} | Ratio | Time | Allocated |"
            : $"| Benchmark | {options.BaseName} | {options.DiffName} | Ratio | Time | Allocated |";

        string separator = showRuntime ? "| :-- | :-- | --: | --: | --: | :-- | :-- |" : "| :-- | --: | --: | --: | :-- | :-- |";

        MarkdownTable table = MarkdownTable.Parse([header, separator]);

        foreach (BenchmarkChange change in changes)
        {
            string name = $"`{change.Current.Name}`";
            string baseTime = Duration(change.Baseline.Median);
            string diffTime = Duration(change.Current.Median);
            string ratio = change.Ratio.ToString("F2", CultureInfo.InvariantCulture);
            string allocated = change.Baseline.Allocated == change.Current.Allocated
                ? Bytes(change.Current.Allocated)
                : $"{Bytes(change.Baseline.Allocated)} → {Bytes(change.Current.Allocated)}";

            if (showRuntime)
                table.Insert(table.RowCount, name, change.Current.Job, baseTime, diffTime, ratio, change.Verdict, allocated);
            else
                table.Insert(table.RowCount, name, baseTime, diffTime, ratio, change.Verdict, allocated);
        }

        return table.Render();
    }

    private static string Duration(double nanoseconds)
    {
        (double value, string unit) = nanoseconds switch
        {
            < 1_000 => (nanoseconds, "ns"),
            < 1_000_000 => (nanoseconds / 1_000, "μs"),
            < 1_000_000_000 => (nanoseconds / 1_000_000, "ms"),
            _ => (nanoseconds / 1_000_000_000, "s"),
        };

        string format = value switch
        {
            < 10 => "F2",
            < 100 => "F1",
            _ => "F0",
        };

        return $"{value.ToString(format, CultureInfo.InvariantCulture)} {unit}";
    }

    private static string Bytes(long? bytes) => bytes switch
    {
        null => "?",
        < 1024 => $"{bytes} B",
        _ => string.Create(CultureInfo.InvariantCulture, $"{bytes / 1024.0:F1} KB"),
    };
}
