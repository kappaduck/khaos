// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos;

/// <summary>
/// Represents where a clock of the game stands: the fixed clock in <see cref="Game2D.FixedUpdate"/>, the frame clock
/// in <see cref="Game2D.Update"/>.
/// </summary>
public readonly struct GameTime
{
    internal GameTime(float delta, double total, long count)
    {
        Delta = delta;
        Total = total;
        Count = count;
    }

    /// <summary>
    /// Gets how many times this clock has ticked, this tick included: the step in <see cref="Game2D.FixedUpdate"/>, the
    /// frame in <see cref="Game2D.Update"/>.
    /// </summary>
    public long Count { get; }

    /// <summary>
    /// Gets the time since the previous tick, in seconds. In <see cref="Game2D.FixedUpdate"/>, it is always the same.
    /// </summary>
    public float Delta { get; }

    /// <summary>
    /// Gets the time this clock has counted since the game started, in seconds.
    /// </summary>
    public double Total { get; }
}
