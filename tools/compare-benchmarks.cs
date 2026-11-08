#!/usr/bin/env dotnet

// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:package Perfolizer
#:include MarkdownTable.cs

using Khaos.Utils;
using Perfolizer.Mathematics.Common;
using Perfolizer.Mathematics.SignificanceTesting;
using Perfolizer.Mathematics.SignificanceTesting.MannWhitney;
using Perfolizer.Metrology;
using Pragmastat;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using Threshold = Perfolizer.Metrology.Threshold;

const string Prefix = "KappaDuck.Khaos.Benchmarks.";
const string DefaultJob = "DefaultJob";
const string SelfTestData = "tools/benchmarks";

try
{
    if (args is ["--self-test"])
        return SelfTest();

    if (!Options.TryParse(args, out Options? options))
    {
        Console.Error.WriteLine("usage: dotnet tools/compare-benchmarks.cs --base <folder or file> --diff <folder or file> [--base-name <name>] [--diff-name <name>] [--threshold 5%] [--strict]");
        Console.Error.WriteLine("       dotnet tools/compare-benchmarks.cs --self-test");
        return 2;
    }

    return Run(options, Console.Out);
}
catch (Exception ex) when (ex is IOException or JsonException or KeyNotFoundException or FormatException)
{
    Console.Error.WriteLine($"error: {ex.Message}");
    return 2;
}

static string Bytes(long? bytes) => bytes switch
{
    null => "?",
    < 1024 => $"{bytes} B",
    _ => string.Create(CultureInfo.InvariantCulture, $"{bytes / 1024.0:F1} KB"),
};

static List<Change> Compare(Dictionary<string, BenchmarkResult> before, Dictionary<string, BenchmarkResult> after, Threshold threshold)
{
    List<Change> changes = [];

    foreach ((string key, BenchmarkResult current) in after)
    {
        if (!before.TryGetValue(key, out BenchmarkResult? baseline))
            continue;

        string verdict = Verdict(baseline, current, threshold);
        bool allocationChanged = baseline.Allocated != current.Allocated;

        if (verdict is "Slower" or "Faster" || allocationChanged)
            changes.Add(new Change(baseline, current, verdict));
    }

    changes.Sort(static (x, y) =>
    {
        int byRatio = y.Ratio.CompareTo(x.Ratio);
        return byRatio != 0 ? byRatio : string.CompareOrdinal(x.Current.Name, y.Current.Name);
    });

    return changes;
}

static string Duration(double nanoseconds)
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

static int FirstDifference(string[] expected, string[] actual)
{
    int length = Math.Max(expected.Length, actual.Length);

    for (int index = 0; index < length; index++)
    {
        if (LineAt(expected, index) != LineAt(actual, index))
            return index;
    }

    return -1;
}

static string LineAt(string[] lines, int index) => index < lines.Length ? lines[index] : "(no line)";

static Dictionary<string, BenchmarkResult> Load(string path)
{
    Dictionary<string, BenchmarkResult> results = [];
    IEnumerable<string> files = File.Exists(path) ? [path] : Directory.EnumerateFiles(path, "*-report-full*.json");

    foreach (string file in files)
    {
        using FileStream stream = File.OpenRead(file);
        using JsonDocument document = JsonDocument.Parse(stream);

        foreach (JsonElement benchmark in document.RootElement.GetProperty("Benchmarks").EnumerateArray())
        {
            BenchmarkResult result = BenchmarkResult.From(benchmark, Prefix, DefaultJob);
            results[result.Key] = result;
        }
    }

    return results;
}

static void Render(TextWriter output, Options options, List<Change> changes, Dictionary<string, BenchmarkResult> before, Dictionary<string, BenchmarkResult> after, int allocationsUp)
{
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
}

static int Run(Options options, TextWriter output)
{
    Dictionary<string, BenchmarkResult> before = Load(options.Base);
    Dictionary<string, BenchmarkResult> after = Load(options.Diff);

    List<Change> changes = Compare(before, after, Threshold.Parse(options.Threshold));
    int allocationsUp = changes.Count(static change => change.AllocationWentUp);

    Render(output, options, changes, before, after, allocationsUp);

    return options.Strict && allocationsUp > 0 ? 1 : 0;
}

static int SelfTest()
{
    Options options = new($"{SelfTestData}/base.json", $"{SelfTestData}/diff.json", "0.1.0", "0.2.0", "5%", Strict: true);
    using StringWriter output = new() { NewLine = "\n" };

    int exitCode = Run(options, output);

    string[] expected = File.ReadAllText($"{SelfTestData}/expected.md").ReplaceLineEndings("\n").Split('\n');
    string[] actual = output.ToString().Split('\n');
    int difference = FirstDifference(expected, actual);

    if (difference >= 0)
    {
        Console.Error.WriteLine($"self-test failed: line {difference + 1} differs from {SelfTestData}/expected.md");
        Console.Error.WriteLine($"  expected: {LineAt(expected, difference)}");
        Console.Error.WriteLine($"  actual:   {LineAt(actual, difference)}");
        return 1;
    }

    if (exitCode != 1)
    {
        Console.Error.WriteLine($"self-test failed: --strict returned {exitCode} instead of 1 for the extra allocation in {SelfTestData}/diff.json");
        return 1;
    }

    Console.WriteLine("Self-test passed: the comparison matches expected.md, and --strict fails on the extra allocation.");
    return 0;
}

static string[] Table(Options options, List<Change> changes)
{
    bool showRuntime = changes.Exists(static change => change.Current.Job != DefaultJob);

    string header = showRuntime
        ? $"| Benchmark | Runtime | {options.BaseName} | {options.DiffName} | Ratio | Time | Allocated |"
        : $"| Benchmark | {options.BaseName} | {options.DiffName} | Ratio | Time | Allocated |";

    string separator = showRuntime ? "| :-- | :-- | --: | --: | --: | :-- | :-- |" : "| :-- | --: | --: | --: | :-- | :-- |";

    MarkdownTable table = MarkdownTable.Parse([header, separator]);

    foreach (Change change in changes)
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

static string Verdict(BenchmarkResult baseline, BenchmarkResult current, Threshold threshold)
{
    if (baseline.Values.AsSpan().SequenceEqual(current.Values))
        return "Same";

    if (baseline.Values.Length < 2 || current.Values.Length < 2)
        return "?";

#pragma warning disable CS0618
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

internal sealed record BenchmarkResult(string Key, string Name, string Job, double[] Values, double Median, long? Allocated)
{
    public string Label => Job == "DefaultJob" ? Name : $"{Name} ({Job})";

    internal static BenchmarkResult From(JsonElement benchmark, string prefix, string defaultJob)
    {
        string fullName = benchmark.GetProperty("FullName").GetString()!;
        string name = fullName.StartsWith(prefix, StringComparison.Ordinal) ? fullName[prefix.Length..] : fullName;
        string job = JobOf(benchmark.GetProperty("DisplayInfo").GetString()!, defaultJob);

        JsonElement statistics = benchmark.GetProperty("Statistics");
        double[] values = [.. statistics.GetProperty("OriginalValues").EnumerateArray().Select(static value => value.GetDouble())];
        double median = statistics.GetProperty(nameof(Median)).GetDouble();

        long? allocated = benchmark.TryGetProperty("Memory", out JsonElement memory) && memory.ValueKind == JsonValueKind.Object
            ? memory.GetProperty("BytesAllocatedPerOperation").GetInt64()
            : null;

        return new BenchmarkResult($"{fullName} | {job}", name, job, values, median, allocated);
    }

    private static string JobOf(string displayInfo, string defaultJob)
    {
        int colon = displayInfo.IndexOf(": ", StringComparison.Ordinal);

        if (colon < 0)
            return defaultJob;

        ReadOnlySpan<char> job = displayInfo.AsSpan(colon + 2);
        int parameters = job.IndexOf(" [", StringComparison.Ordinal);

        return (parameters < 0 ? job : job[..parameters]).ToString();
    }
}

internal sealed record Change(BenchmarkResult Baseline, BenchmarkResult Current, string Verdict)
{
    public bool AllocationWentUp => Current.Allocated > Baseline.Allocated;

    public double Ratio => Current.Median / Baseline.Median;
}

internal sealed record Options(string Base, string Diff, string BaseName, string DiffName, string Threshold, bool Strict)
{
    internal static bool TryParse(ReadOnlySpan<string> args, [NotNullWhen(true)] out Options? options)
    {
        options = null;

        string? baseFolder = null;
        string? diffFolder = null;
        string baseName = "base";
        string diffName = "diff";
        string threshold = "5%";
        bool strict = false;

        int index = 0;

        while (index < args.Length)
        {
            string name = args[index++];

            if (name == "--strict")
            {
                strict = true;
                continue;
            }

            if (index == args.Length)
                return false;

            string value = args[index++];

            switch (name)
            {
                case "--base":
                    baseFolder = value;
                    break;

                case "--diff":
                    diffFolder = value;
                    break;

                case "--base-name":
                    baseName = value;
                    break;

                case "--diff-name":
                    diffName = value;
                    break;

                case "--threshold":
                    threshold = value;
                    break;

                default:
                    return false;
            }
        }

        if (baseFolder is null || diffFolder is null)
            return false;

        options = new Options(baseFolder, diffFolder, baseName, diffName, threshold, strict);
        return true;
    }
}
