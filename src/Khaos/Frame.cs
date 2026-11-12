// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos;

/// <summary>
/// Represents the frame being drawn, given to <see cref="Game2D.Draw"/>.
/// </summary>
public readonly struct Frame
{
    internal Frame(float alpha) => Alpha = alpha;

    /// <summary>
    /// Gets how far the frame is between the last fixed step and the next one, from 0 (right on the last step) to just
    /// under 1.
    /// </summary>
    /// <remarks>
    /// Draw a moving thing at <c>Vector2.Lerp(previous, current, frame.Alpha)</c> so it moves smoothly on any screen.
    /// </remarks>
    public float Alpha { get; }
}
