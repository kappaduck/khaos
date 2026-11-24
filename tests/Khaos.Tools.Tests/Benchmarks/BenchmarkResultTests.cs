// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Benchmarks;
using System.Text.Json;
using TUnit.Assertions.Enums;

namespace KappaDuck.Khaos.Tools.Tests.Benchmarks;

internal sealed class BenchmarkResultTests
{
    private const string Prefix = "KappaDuck.Khaos.Benchmarks.";
    private const string Union = "KappaDuck.Khaos.Benchmarks.Micro.Mathematics.RectBenchmarks.Union";
    private const string DefaultJob = "RectBenchmarks.Union: DefaultJob";

    [Test]
    public async Task FromShouldRemoveThePrefixFromTheName()
    {
        BenchmarkEntry benchmark = JsonSerializer.Deserialize($$"""
            {
              "DisplayInfo": "{{DefaultJob}}",
              "FullName": "{{Union}}",
              "Statistics": { "OriginalValues": [3.0, 2.0, 4.0], "Median": 3.0 }
            }
            """, BenchmarkJsonContext.Default.BenchmarkEntry)!;

        BenchmarkResult result = BenchmarkResult.From(benchmark, Prefix);

        await result.Name.Should().BeEqualTo("Micro.Mathematics.RectBenchmarks.Union");
    }

    [Test]
    public async Task FromShouldKeepANameWithoutThePrefix()
    {
        BenchmarkEntry benchmark = JsonSerializer.Deserialize($$"""
            {
              "DisplayInfo": "{{DefaultJob}}",
              "FullName": "Other.RectBenchmarks.Union",
              "Statistics": { "OriginalValues": [3.0, 2.0, 4.0], "Median": 3.0 }
            }
            """, BenchmarkJsonContext.Default.BenchmarkEntry)!;

        BenchmarkResult result = BenchmarkResult.From(benchmark, Prefix);

        await result.Name.Should().BeEqualTo("Other.RectBenchmarks.Union");
    }

    [Test]
    [Arguments("RectBenchmarks.Union: DefaultJob", "DefaultJob")]
    [Arguments("RectBenchmarks.Union: ShortRun", "ShortRun")]
    [Arguments("FrameBenchmarks.FrameWithEvents: ShortRun [Count=64]", "ShortRun")]
    [Arguments("RectBenchmarks.Union", "DefaultJob")]
    public async Task FromShouldReadTheJobFromTheDisplayInfo(string displayInfo, string expected)
    {
        BenchmarkEntry benchmark = JsonSerializer.Deserialize($$"""
            {
              "DisplayInfo": "{{displayInfo}}",
              "FullName": "{{Union}}",
              "Statistics": { "OriginalValues": [3.0, 2.0, 4.0], "Median": 3.0 }
            }
            """, BenchmarkJsonContext.Default.BenchmarkEntry)!;

        BenchmarkResult result = BenchmarkResult.From(benchmark, Prefix);

        await result.Job.Should().BeEqualTo(expected);
    }

    [Test]
    public async Task FromShouldKeyTheResultByFullNameAndJob()
    {
        BenchmarkEntry benchmark = JsonSerializer.Deserialize($$"""
            {
              "DisplayInfo": "RectBenchmarks.Union: ShortRun",
              "FullName": "{{Union}}",
              "Statistics": { "OriginalValues": [3.0, 2.0, 4.0], "Median": 3.0 }
            }
            """, BenchmarkJsonContext.Default.BenchmarkEntry)!;

        BenchmarkResult result = BenchmarkResult.From(benchmark, Prefix);

        await result.Key.Should().BeEqualTo($"{Union} | ShortRun");
    }

    [Test]
    public async Task FromShouldReadTheValuesAndTheMedian()
    {
        BenchmarkEntry benchmark = JsonSerializer.Deserialize($$"""
            {
              "DisplayInfo": "{{DefaultJob}}",
              "FullName": "{{Union}}",
              "Statistics": { "OriginalValues": [3.0, 2.0, 4.0], "Median": 3.0 }
            }
            """, BenchmarkJsonContext.Default.BenchmarkEntry)!;

        BenchmarkResult result = BenchmarkResult.From(benchmark, Prefix);

        await result.Values.Should().BeEquivalentTo([3.0, 2.0, 4.0], EqualityComparer<double>.Default, CollectionOrdering.Matching);
        await result.Median.Should().BeEqualTo(3.0);
    }

    [Test]
    public async Task FromShouldReadTheBytesAllocatedPerOperation()
    {
        BenchmarkEntry benchmark = JsonSerializer.Deserialize($$"""
            {
              "DisplayInfo": "{{DefaultJob}}",
              "FullName": "{{Union}}",
              "Memory": { "BytesAllocatedPerOperation": 48 },
              "Statistics": { "OriginalValues": [3.0, 2.0, 4.0], "Median": 3.0 }
            }
            """, BenchmarkJsonContext.Default.BenchmarkEntry)!;

        BenchmarkResult result = BenchmarkResult.From(benchmark, Prefix);

        await result.Allocated.Should().BeEqualTo(48L);
    }

    [Test]
    [Arguments("")]
    [Arguments("\"Memory\": null,")]
    public async Task FromShouldGiveNoAllocationWithoutTheMemoryDiagnoser(string memory)
    {
        BenchmarkEntry benchmark = JsonSerializer.Deserialize($$"""
            {
              "DisplayInfo": "{{DefaultJob}}",
              "FullName": "{{Union}}",
              {{memory}}
              "Statistics": { "OriginalValues": [3.0, 2.0, 4.0], "Median": 3.0 }
            }
            """, BenchmarkJsonContext.Default.BenchmarkEntry)!;

        BenchmarkResult result = BenchmarkResult.From(benchmark, Prefix);

        await result.Allocated.Should().BeNull();
    }

    [Test]
    public async Task LabelShouldBeTheNameForTheDefaultJob()
    {
        BenchmarkEntry benchmark = JsonSerializer.Deserialize($$"""
            {
              "DisplayInfo": "{{DefaultJob}}",
              "FullName": "{{Union}}",
              "Statistics": { "OriginalValues": [3.0, 2.0, 4.0], "Median": 3.0 }
            }
            """, BenchmarkJsonContext.Default.BenchmarkEntry)!;

        BenchmarkResult result = BenchmarkResult.From(benchmark, Prefix);

        await result.Label.Should().BeEqualTo("Micro.Mathematics.RectBenchmarks.Union");
    }

    [Test]
    public async Task LabelShouldNameAnyOtherJob()
    {
        BenchmarkEntry benchmark = JsonSerializer.Deserialize($$"""
            {
              "DisplayInfo": "RectBenchmarks.Union: ShortRun",
              "FullName": "{{Union}}",
              "Statistics": { "OriginalValues": [3.0, 2.0, 4.0], "Median": 3.0 }
            }
            """, BenchmarkJsonContext.Default.BenchmarkEntry)!;

        BenchmarkResult result = BenchmarkResult.From(benchmark, Prefix);

        await result.Label.Should().BeEqualTo("Micro.Mathematics.RectBenchmarks.Union (ShortRun)");
    }

    [Test]
    public async Task RatioShouldDivideTheNewMedianByTheBaseMedian()
    {
        BenchmarkResult before = new("key", "Micro.RectBenchmarks.Union", BenchmarkResult.DefaultJob, [2.0], 2.0, null);
        BenchmarkResult after = new("key", "Micro.RectBenchmarks.Union", BenchmarkResult.DefaultJob, [3.0], 3.0, null);

        BenchmarkChange change = new(before, after, "Slower");

        await change.Ratio.Should().BeEqualTo(1.5);
    }

    [Test]
    [Arguments(0L, 48L, true)]
    [Arguments(48L, 0L, false)]
    [Arguments(48L, 48L, false)]
    [Arguments(null, 48L, false)]
    public async Task AllocationWentUpShouldOnlyReturnTrueWhenBothAreMeasuredAndTheNewOneIsHigher(long? before, long? after, bool expected)
    {
        BenchmarkResult first = new("key", "Micro.RectBenchmarks.Union", BenchmarkResult.DefaultJob, [1.0], 1.0, before);
        BenchmarkResult second = new("key", "Micro.RectBenchmarks.Union", BenchmarkResult.DefaultJob, [1.0], 1.0, after);

        BenchmarkChange change = new(first, second, "Same");

        await change.AllocationWentUp.Should().BeEqualTo(expected);
    }
}
