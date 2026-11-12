// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using KappaDuck.Khaos.Hosting.Engine;

namespace KappaDuck.Khaos.Benchmarks.Frames;

public class FrameBenchmarks
{
    private KhaosEngine _engine = null!;

    private int _frame;

    [GlobalSetup]
    public void StartEngine() => _engine = Headless.Start(new FrameGame());

    [GlobalCleanup]
    public void StopEngine() => _engine.Dispose();

    [Benchmark(Baseline = true)]
    public void EmptyFrame() => _engine.Iterate();

    [Benchmark]
    public void FrameWithTitleCounter()
    {
        _engine.Window.SetTitle($"Benchmarks - frame {_frame++}");
        _engine.Iterate();
    }

    [Benchmark]
    public void FrameWithTitleString()
    {
        _engine.Window.Title = "Benchmarks - frame " + _frame++;
        _engine.Iterate();
    }
}
