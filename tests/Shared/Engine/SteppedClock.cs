// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Testing.Engine;

internal sealed class SteppedClock : TimeProvider
{
    internal const long Frequency = 720;
    internal const long TicksPerFrameAt144Hz = Frequency / 144;
    internal const long TicksPerFrameAt30Hz = Frequency / 30;
    internal const long TicksPerStepAt60Hz = Frequency / 60;

    private long _now;

    internal long TicksPerRead { get; init; }

    public override long TimestampFrequency => Frequency;

    public override long GetTimestamp()
    {
        long now = _now;
        _now += TicksPerRead;

        return now;
    }

    internal void Advance(long ticks) => _now += ticks;
}
