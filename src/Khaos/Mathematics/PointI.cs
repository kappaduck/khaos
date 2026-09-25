// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Numerics;

namespace KappaDuck.Khaos.Mathematics;

/// <summary>
/// Represents a location in integer coordinates, such as a pixel, a grid cell or a display position.
/// </summary>
/// <remarks>
/// Subtracting two points gives the <see cref="Vector2"/> between them. To step to a neighboring cell,
/// use <see cref="Offset(int, int)"/>.
/// </remarks>
/// <param name="x">The x-coordinate.</param>
/// <param name="y">The y-coordinate.</param>
[StructLayout(LayoutKind.Sequential)]
public readonly struct PointI(int x, int y) :
    IAdditionOperators<PointI, Vector2, Point>,
    ISubtractionOperators<PointI, Vector2, Point>,
    ISubtractionOperators<PointI, PointI, Vector2>,
    IEqualityOperators<PointI, PointI, bool>,
    IEquatable<PointI>,
    ISpanFormattable,
    IUtf8SpanFormattable
{
    /// <summary>
    /// Gets the point at (0, 0).
    /// </summary>
    public static PointI Origin { get; }

    /// <summary>
    /// Gets the x-coordinate.
    /// </summary>
    public int X { get; init; } = x;

    /// <summary>
    /// Gets the y-coordinate.
    /// </summary>
    public int Y { get; init; } = y;

    /// <summary>
    /// Computes the distance between two points.
    /// </summary>
    /// <remarks>
    /// Computed in floating point, so it does not overflow for points far apart.
    /// </remarks>
    /// <param name="from">The first point.</param>
    /// <param name="to">The second point.</param>
    /// <returns>The distance between the two points.</returns>
    public static float Distance(PointI from, PointI to) => (to - from).Length();

    /// <summary>
    /// Returns this point moved by a whole number of units on each axis.
    /// </summary>
    /// <param name="dx">The horizontal offset.</param>
    /// <param name="dy">The vertical offset.</param>
    /// <returns>The moved point.</returns>
    public PointI Offset(int dx, int dy) => new(X + dx, Y + dy);

    /// <summary>
    /// Deconstructs the point into its coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    public void Deconstruct(out int x, out int y) => (x, y) = (X, Y);

    /// <inheritdoc/>
    public bool Equals(PointI other) => X == other.X && Y == other.Y;

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is PointI other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(X, Y);

    /// <summary>
    /// Returns the point as <c>(x, y)</c>, using the current culture.
    /// </summary>
    /// <returns>The formatted point.</returns>
    public override string ToString() => ToString(null, null);

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider)
        => GeometryFormatting.ToString(this, format, formatProvider);

    /// <inheritdoc/>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => GeometryFormatting.TryFormat(destination, out charsWritten, [X, Y], format, provider);

    /// <inheritdoc/>
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => GeometryFormatting.TryFormat(utf8Destination, out bytesWritten, [X, Y], format, provider);

    /// <summary>
    /// Moves a point by a displacement, giving a floating-point location.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="displacement">The displacement.</param>
    /// <returns>The moved point.</returns>
    public static Point operator +(PointI point, Vector2 displacement) => new(point.X + displacement.X, point.Y + displacement.Y);

    /// <summary>
    /// Moves a point backward by a displacement, giving a floating-point location.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="displacement">The displacement.</param>
    /// <returns>The moved point.</returns>
    public static Point operator -(PointI point, Vector2 displacement) => new(point.X - displacement.X, point.Y - displacement.Y);

    /// <summary>
    /// Computes the displacement from one point to another.
    /// </summary>
    /// <remarks>
    /// Computed in floating point, so it does not overflow for points far apart.
    /// </remarks>
    /// <param name="to">The point the displacement ends at.</param>
    /// <param name="from">The point the displacement starts from.</param>
    /// <returns>The displacement that moves <paramref name="from"/> onto <paramref name="to"/>.</returns>
    public static Vector2 operator -(PointI to, PointI from) => new((float)to.X - from.X, (float)to.Y - from.Y);

    /// <summary>
    /// Determines whether two points are equal.
    /// </summary>
    /// <param name="left">The first point.</param>
    /// <param name="right">The second point.</param>
    /// <returns><see langword="true"/> if the points are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(PointI left, PointI right) => left.Equals(right);

    /// <summary>
    /// Determines whether two points differ.
    /// </summary>
    /// <param name="left">The first point.</param>
    /// <param name="right">The second point.</param>
    /// <returns><see langword="true"/> if the points differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(PointI left, PointI right) => !left.Equals(right);

    /// <summary>
    /// Converts an integer point to a floating-point point.
    /// </summary>
    /// <remarks>
    /// Exact for coordinates up to 2^24 (16,777,216) in magnitude, like any <see cref="int"/> to
    /// <see cref="float"/> conversion.
    /// </remarks>
    /// <param name="point">The point to convert.</param>
    public static implicit operator Point(PointI point) => new(point.X, point.Y);

    /// <summary>
    /// Converts a point to the displacement from the origin.
    /// </summary>
    /// <param name="point">The point.</param>
    public static explicit operator Vector2(PointI point) => new(point.X, point.Y);
}
