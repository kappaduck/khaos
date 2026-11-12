// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Benchmarks.Frames;

internal sealed class BenchmarkClock : TimeProvider
{
    internal const long TicksPerStepAt60Hz = Frequency / 60;

    private const long Frequency = 720;

    private long _now;

    public override long TimestampFrequency => Frequency;

    public override long GetTimestamp() => _now;

    internal void Advance(long ticks) => _now += ticks;
}
