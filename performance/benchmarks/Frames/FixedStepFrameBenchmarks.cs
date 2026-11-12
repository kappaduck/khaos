// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using KappaDuck.Khaos.Hosting.Engine;

namespace KappaDuck.Khaos.Benchmarks.Frames;

public class FixedStepFrameBenchmarks
{
    private readonly BenchmarkClock _clock = new();

    private KhaosEngine _engine = null!;

    [Params(0, 1, 2, 4)]
    public int Steps { get; set; }

    [GlobalSetup]
    public void StartEngine() => _engine = Headless.Start(new FrameGame(), _clock);

    [GlobalCleanup]
    public void StopEngine() => _engine.Dispose();

    [Benchmark]
    public void FrameWithFixedSteps()
    {
        _clock.Advance(Steps * BenchmarkClock.TicksPerStepAt60Hz);
        _engine.Iterate();
    }
}
