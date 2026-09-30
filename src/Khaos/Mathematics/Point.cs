// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Numerics;

namespace KappaDuck.Khaos.Mathematics;

/// <summary>
/// Represents a location in floating-point coordinates, such as a position in the world or on screen.
/// </summary>
/// <remarks>
/// A point is a location, a <see cref="Vector2"/> is a displacement. Subtracting two points gives the
/// <see cref="Vector2"/> between them, and adding a <see cref="Vector2"/> to a point moves it.
/// </remarks>
/// <param name="x">The x-coordinate.</param>
/// <param name="y">The y-coordinate.</param>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Point(float x, float y) :
    IAdditionOperators<Point, Vector2, Point>,
    ISubtractionOperators<Point, Vector2, Point>,
    ISubtractionOperators<Point, Point, Vector2>,
    IEqualityOperators<Point, Point, bool>,
    IEquatable<Point>,
    ISpanFormattable,
    IUtf8SpanFormattable
{
    /// <summary>
    /// Gets the point at (0, 0).
    /// </summary>
    public static Point Origin { get; }

    /// <summary>
    /// Gets the x-coordinate.
    /// </summary>
    public float X { get; init; } = x;

    /// <summary>
    /// Gets the y-coordinate.
    /// </summary>
    public float Y { get; init; } = y;

    /// <summary>
    /// Computes the distance between two points.
    /// </summary>
    /// <param name="from">The first point.</param>
    /// <param name="to">The second point.</param>
    /// <returns>The distance between the two points.</returns>
    public static float Distance(Point from, Point to) => (to - from).Length();

    /// <summary>
    /// Computes the squared distance between two points.
    /// </summary>
    /// <remarks>
    /// Cheaper than <see cref="Distance(Point, Point)"/>: compare it to a squared radius to test a range
    /// without a square root.
    /// </remarks>
    /// <param name="from">The first point.</param>
    /// <param name="to">The second point.</param>
    /// <returns>The squared distance between the two points.</returns>
    public static float DistanceSquared(Point from, Point to) => (to - from).LengthSquared();

    /// <summary>
    /// Interpolates linearly between two points.
    /// </summary>
    /// <remarks>
    /// Not clamped, like <see cref="Vector2.Lerp(Vector2, Vector2, float)"/>: an <paramref name="amount"/>
    /// of 2 lands as far past <paramref name="to"/> as <paramref name="from"/> is before it.
    /// </remarks>
    /// <param name="from">The point at <paramref name="amount"/> 0.</param>
    /// <param name="to">The point at <paramref name="amount"/> 1.</param>
    /// <param name="amount">The interpolation factor.</param>
    /// <returns>The interpolated point.</returns>
    public static Point Lerp(Point from, Point to, float amount) => from + ((to - from) * amount);

    /// <summary>
    /// Returns the point with each coordinate rounded down.
    /// </summary>
    /// <returns>The rounded point.</returns>
    public PointI Floor() => new((int)MathF.Floor(X), (int)MathF.Floor(Y));

    /// <summary>
    /// Returns the point with each coordinate rounded up.
    /// </summary>
    /// <returns>The rounded point.</returns>
    public PointI Ceiling() => new((int)MathF.Ceiling(X), (int)MathF.Ceiling(Y));

    /// <summary>
    /// Returns the point with each coordinate rounded to the nearest integer, halves away from zero.
    /// </summary>
    /// <remarks>
    /// Halves go away from zero (0.5 → 1, 1.5 → 2, 2.5 → 3) rather than to the nearest even integer, so
    /// snapping to pixels does not alternate direction from one coordinate to the next.
    /// </remarks>
    /// <returns>The rounded point.</returns>
    public PointI Round()
        => new((int)MathF.Round(X, MidpointRounding.AwayFromZero), (int)MathF.Round(Y, MidpointRounding.AwayFromZero));

    /// <summary>
    /// Returns the point with each coordinate truncated toward zero.
    /// </summary>
    /// <returns>The truncated point.</returns>
    public PointI Truncate() => new((int)X, (int)Y);

    /// <summary>
    /// Determines whether this point is within <paramref name="tolerance"/> of another on both axes.
    /// </summary>
    /// <param name="other">The point to compare with.</param>
    /// <param name="tolerance">The largest difference allowed on each axis.</param>
    /// <returns><see langword="true"/> if both coordinates are within <paramref name="tolerance"/>; otherwise, <see langword="false"/>.</returns>
    public bool IsApproximately(Point other, float tolerance)
        => MathF.Abs(X - other.X) <= tolerance && MathF.Abs(Y - other.Y) <= tolerance;

    /// <summary>
    /// Deconstructs the point into its coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    public void Deconstruct(out float x, out float y) => (x, y) = (X, Y);

    /// <inheritdoc/>
    /// <remarks>
    /// The comparison is exact. To compare computed values with a tolerance, use
    /// <see cref="IsApproximately(Point, float)"/>.
    /// </remarks>
#pragma warning disable S1244 // Exact equality is the contract; IsApproximately is the tolerant comparison.
    public bool Equals(Point other) => X.Equals(other.X) && Y.Equals(other.Y);
#pragma warning restore S1244

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Point other && Equals(other);

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
    /// Moves a point by a displacement.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="displacement">The displacement.</param>
    /// <returns>The moved point.</returns>
    public static Point operator +(Point point, Vector2 displacement) => new(point.X + displacement.X, point.Y + displacement.Y);

    /// <summary>
    /// Moves a point backward by a displacement.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="displacement">The displacement.</param>
    /// <returns>The moved point.</returns>
    public static Point operator -(Point point, Vector2 displacement) => new(point.X - displacement.X, point.Y - displacement.Y);

    /// <summary>
    /// Computes the displacement from one point to another.
    /// </summary>
    /// <param name="to">The point the displacement ends at.</param>
    /// <param name="from">The point the displacement starts from.</param>
    /// <returns>The displacement that moves <paramref name="from"/> onto <paramref name="to"/>.</returns>
    public static Vector2 operator -(Point to, Point from) => new(to.X - from.X, to.Y - from.Y);

    /// <summary>
    /// Determines whether two points are exactly equal.
    /// </summary>
    /// <param name="left">The first point.</param>
    /// <param name="right">The second point.</param>
    /// <returns><see langword="true"/> if the points are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Point left, Point right) => left.Equals(right);

    /// <summary>
    /// Determines whether two points differ.
    /// </summary>
    /// <param name="left">The first point.</param>
    /// <param name="right">The second point.</param>
    /// <returns><see langword="true"/> if the points differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Point left, Point right) => !left.Equals(right);

    /// <summary>
    /// Converts a point to the displacement from the origin.
    /// </summary>
    /// <param name="point">The point.</param>
    public static explicit operator Vector2(Point point) => new(point.X, point.Y);

    /// <summary>
    /// Converts a displacement from the origin to the point it lands on.
    /// </summary>
    /// <param name="vector">The displacement.</param>
    public static explicit operator Point(Vector2 vector) => new(vector.X, vector.Y);
}
