// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Hosting.Engine;

internal sealed class LoopClock
{
    private readonly long _frequency;
    private readonly long _maxElapsed;
    private readonly int _rate;
    private readonly float _stepDelta;

    private long _accumulated;
    private long _frames;
    private long _last;
    private long _steps;
    private double _total;

    internal LoopClock(LoopOptions options, long frequency)
    {
        _frequency = frequency;
        _maxElapsed = (long)(options.MaxFrameTime * frequency);
        _rate = options.FixedRate;
        _stepDelta = 1f / options.FixedRate;
    }

    internal float Alpha => (float)((double)_accumulated / _frequency);

    internal GameTime Frame { get; private set; }

    internal int Advance(long now)
    {
        long elapsed = Math.Clamp(now - _last, 0, _maxElapsed);
        _last = now;

        _accumulated += elapsed * _rate;
        long steps = _accumulated / _frequency;
        _accumulated -= steps * _frequency;

        double seconds = (double)elapsed / _frequency;
        _total += seconds;
        _frames++;
        Frame = new GameTime((float)seconds, _total, _frames);

        return (int)steps;
    }

    internal GameTime NextStep()
    {
        _steps++;
        return new GameTime(_stepDelta, (double)_steps / _rate, _steps);
    }

    internal void Resume(long now) => _last = now;

    internal void Start(long now) => _last = now;
}
