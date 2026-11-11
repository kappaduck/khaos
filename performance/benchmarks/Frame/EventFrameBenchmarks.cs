// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using KappaDuck.Khaos.Hosting;

namespace KappaDuck.Khaos.Benchmarks.Frame;

public class EventFrameBenchmarks
{
    private KhaosEngine _engine = null!;

    private int _frame;

    [Params(0, 8, 64)]
    public int Events { get; set; }

    [GlobalSetup]
    public void StartEngine()
    {
        Headless.UseOffscreen();

        _engine = KhaosEngine.Start(new FrameGame());
    }

    [GlobalCleanup]
    public void StopEngine() => _engine.Dispose();

    [Benchmark]
    public void FrameWithEvents()
    {
        QueuedEvents.Push(Events, _frame++);
        QueuedEvents.Dispatch(_engine);
        _engine.Iterate();
    }
}
