// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Tools.Benchmarks;

internal sealed record BenchmarkChange(BenchmarkResult Baseline, BenchmarkResult Current, string Verdict)
{
    public bool AllocationWentUp => Current.Allocated > Baseline.Allocated;

    public double Ratio => Current.Median / Baseline.Median;
}
