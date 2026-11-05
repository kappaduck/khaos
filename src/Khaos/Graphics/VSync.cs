// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Globalization;
using System.Numerics;

namespace KappaDuck.Khaos.Graphics;

/// <summary>
/// Represents how the game waits for the screen before it shows each frame, which is called vertical sync (VSync).
/// </summary>
/// <remarks>
/// <para>
/// With <see cref="Enabled"/>, the default, the game shows one frame each time the screen refreshes, so 60 frames
/// per second on a 60 Hz screen. With <see cref="Disabled"/>, it draws as fast as it can, and the picture can tear.
/// </para>
/// <para>
/// In the browser, the page always draws at the screen's rate, and this does nothing.
/// </para>
/// </remarks>
public readonly struct VSync : IEqualityOperators<VSync, VSync, bool>, IEquatable<VSync>
{
    private const int AdaptiveInterval = -1;
    private const string AdaptiveName = "adaptive";

    private VSync(int interval) => Interval = interval;

    /// <summary>
    /// Gets the VSync that waits for the screen, but shows a late frame right away instead of waiting for the next
    /// refresh.
    /// </summary>
    /// <remarks>
    /// A late frame can tear the picture a little, but the game does not stutter. Where the graphics card does not
    /// support it, the game uses <see cref="Enabled"/>.
    /// </remarks>
    public static VSync Adaptive => new(AdaptiveInterval);

    /// <summary>
    /// Gets the VSync that does not wait for the screen, so the game draws as fast as it can.
    /// </summary>
    public static VSync Disabled => default;

    /// <summary>
    /// Gets the VSync that shows one frame for each refresh of the screen.
    /// </summary>
    public static VSync Enabled => new(1);

    internal int Interval { get; }

    /// <summary>
    /// Returns the VSync that shows one frame every few refreshes of the screen.
    /// </summary>
    /// <remarks>
    /// <c>Every(2)</c> shows 30 frames per second on a 60 Hz screen. Where the graphics card does not support it, the
    /// game uses <see cref="Enabled"/>.
    /// </remarks>
    /// <param name="refreshes">The number of refreshes per frame, 1 or more.</param>
    /// <returns>The VSync that waits for <paramref name="refreshes"/> refreshes before each frame.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="refreshes"/> is less than 1.</exception>
    public static VSync Every(int refreshes)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(refreshes);
        return new(refreshes);
    }

    /// <inheritdoc/>
    public bool Equals(VSync other) => Interval == other.Interval;

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is VSync vsync && Equals(vsync);

    /// <inheritdoc/>
    public override int GetHashCode() => Interval.GetHashCode();

    /// <summary>
    /// Returns the name of the VSync, which is <c>Disabled</c>, <c>Enabled</c>, <c>Adaptive</c> or <c>Every(n)</c>.
    /// </summary>
    /// <returns>The name of the VSync.</returns>
    public override string ToString() => Interval switch
    {
        0 => nameof(Disabled),
        1 => nameof(Enabled),
        AdaptiveInterval => nameof(Adaptive),
        _ => $"{nameof(Every)}({Interval.ToString(CultureInfo.InvariantCulture)})",
    };

    /// <inheritdoc/>
    public static bool operator ==(VSync left, VSync right) => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(VSync left, VSync right) => !left.Equals(right);

    internal static bool TryParse(ReadOnlySpan<char> value, out VSync vsync)
    {
        value = value.Trim();

        if (bool.TryParse(value, out bool enabled))
        {
            vsync = enabled ? Enabled : Disabled;
            return true;
        }

        if (value.Equals(AdaptiveName, StringComparison.OrdinalIgnoreCase))
        {
            vsync = Adaptive;
            return true;
        }

        if (int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int refreshes))
        {
            vsync = new VSync(refreshes);
            return true;
        }

        vsync = default;
        return false;
    }
}
