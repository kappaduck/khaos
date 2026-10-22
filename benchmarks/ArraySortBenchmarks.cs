// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;

namespace KappaDuck.Khaos.Benchmarks;

public class ArraySortBenchmarks
{
    private const int Seed = 42;

    private int[] _source = [];

    [Params(100, 1000, 10_000)]
    public int Length { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Random random = new(Seed);

        _source = new int[Length];

        for (int i = 0; i < _source.Length; i++)
            _source[i] = random.Next();
    }

    [Benchmark(Baseline = true)]
    public int[] ArraySort()
    {
        int[] sorted = [.. _source];

        Array.Sort(sorted);
        return sorted;
    }

    [Benchmark]
    public int[] LinqOrder() => [.. _source.Order()];
}
