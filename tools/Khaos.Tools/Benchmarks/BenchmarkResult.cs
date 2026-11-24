// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Tools.Benchmarks;

internal sealed record BenchmarkResult(string Key, string Name, string Job, IReadOnlyList<double> Values, double Median, long? Allocated)
{
    public const string DefaultJob = "DefaultJob";

    public string Label => Job == DefaultJob ? Name : $"{Name} ({Job})";

    public static BenchmarkResult From(BenchmarkEntry benchmark, string prefix)
    {
        ArgumentNullException.ThrowIfNull(benchmark);
        ArgumentNullException.ThrowIfNull(prefix);

        string fullName = benchmark.FullName;
        string name = fullName.StartsWith(prefix, StringComparison.Ordinal) ? fullName[prefix.Length..] : fullName;
        string job = JobOf(benchmark.DisplayInfo);

        return new BenchmarkResult(
            $"{fullName} | {job}",
            name,
            job,
            benchmark.Statistics.OriginalValues,
            benchmark.Statistics.Median,
            benchmark.Memory?.BytesAllocatedPerOperation);
    }

    private static string JobOf(string displayInfo)
    {
        int colon = displayInfo.IndexOf(": ", StringComparison.Ordinal);

        if (colon < 0)
            return DefaultJob;

        ReadOnlySpan<char> job = displayInfo.AsSpan(colon + 2);
        int parameters = job.IndexOf(" [", StringComparison.Ordinal);

        return (parameters < 0 ? job : job[..parameters]).ToString();
    }
}
