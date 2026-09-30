// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Numerics;
using System.Text;

namespace KappaDuck.Khaos.Mathematics;

/// <summary>
/// Represents an angle, stored in radians.
/// </summary>
/// <remarks>
/// <para>
/// Positive angles turn clockwise on screen, because the Y axis points down. This matches
/// <see cref="Transform.Rotation(Angle)"/> and the rotation applies when drawing.
/// </para>
/// <para>
/// Equality is exact. Use <see cref="IsApproximately(Angle, Angle)"/> to compare two computed angles.
/// </para>
/// </remarks>
public readonly struct Angle :
    IAdditionOperators<Angle, Angle, Angle>,
    ISubtractionOperators<Angle, Angle, Angle>,
    IMultiplyOperators<Angle, float, Angle>,
    IDivisionOperators<Angle, float, Angle>,
    IUnaryNegationOperators<Angle, Angle>,
    IComparisonOperators<Angle, Angle, bool>,
    IEquatable<Angle>,
    IComparable<Angle>,
    ISpanFormattable,
    IUtf8SpanFormattable
{
    private const string DegreeSign = "°";

    private Angle(float radians) => Radians = radians;

    /// <summary>
    /// Gets an angle of zero.
    /// </summary>
    public static Angle Zero { get; }

    /// <summary>
    /// Gets the angle in radians.
    /// </summary>
    public float Radians { get; }

    /// <summary>
    /// Gets the angle in degrees.
    /// </summary>
    public float Degrees => float.RadiansToDegrees(Radians);

    /// <summary>
    /// Gets the sine of the angle.
    /// </summary>
    public float Sin => MathF.Sin(Radians);

    /// <summary>
    /// Gets the cosine of the angle.
    /// </summary>
    public float Cos => MathF.Cos(Radians);

    /// <summary>
    /// Gets the tangent of the angle.
    /// </summary>
    public float Tan => MathF.Tan(Radians);

    /// <summary>
    /// Creates an angle from a value in degrees.
    /// </summary>
    /// <param name="degrees">The angle in degrees.</param>
    /// <returns>The angle.</returns>
    public static Angle FromDegrees(float degrees) => new(float.DegreesToRadians(degrees));

    /// <summary>
    /// Creates an angle from a value in radians.
    /// </summary>
    /// <param name="radians">The angle in radians.</param>
    /// <returns>The angle.</returns>
    public static Angle FromRadians(float radians) => new(radians);

    /// <summary>
    /// Wraps the angle into the range [0°, 360°).
    /// </summary>
    /// <returns>The equivalent angle within [0°, 360°).</returns>
    public Angle Normalize()
    {
        float radians = Radians % float.Tau;

        if (radians < 0f)
            radians += float.Tau;

        return new(radians >= float.Tau ? 0f : radians);
    }

    /// <summary>
    /// Wraps the angle into the range [-180°, 180°).
    /// </summary>
    /// <remarks>
    /// Useful to know which way to turn: a negative result turns counter-clockwise on screen, a positive
    /// one clockwise, and the magnitude is never more than half a turn.
    /// </remarks>
    /// <returns>The equivalent angle within [-180°, 180°).</returns>
    public Angle NormalizeSigned()
    {
        float radians = Normalize().Radians;
        return new(radians >= float.Pi ? radians - float.Tau : radians);
    }

    /// <summary>
    /// Determines whether this angle points in approximately the same direction as another.
    /// </summary>
    /// <remarks>
    /// The comparison wraps around: 359.9° and 0.1° are 0.2° apart, and 720° is the same direction as 0°.
    /// Compare <see cref="Radians"/> directly to tell full turns apart.
    /// </remarks>
    /// <param name="other">The angle to compare with.</param>
    /// <param name="tolerance">The largest difference still considered equal.</param>
    /// <returns><see langword="true"/> if the two directions are within <paramref name="tolerance"/>; otherwise, <see langword="false"/>.</returns>
    public bool IsApproximately(Angle other, Angle tolerance)
        => MathF.Abs((this - other).NormalizeSigned().Radians) <= MathF.Abs(tolerance.Radians);

    /// <inheritdoc/>
    public int CompareTo(Angle other) => Radians.CompareTo(other.Radians);

    /// <inheritdoc/>
    /// <remarks>
    /// The comparison is exact. To compare computed values with a tolerance, use
    /// <see cref="IsApproximately(Angle, Angle)"/>.
    /// </remarks>
#pragma warning disable S1244 // Exact equality is the contract; IsApproximately is the tolerant comparison.
    public bool Equals(Angle other) => Radians.Equals(other.Radians);
#pragma warning restore S1244

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Angle other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Radians.GetHashCode();

    /// <summary>
    /// Returns the angle in degrees, such as <c>90°</c>, using the current culture.
    /// </summary>
    /// <returns>The angle in degrees.</returns>
    public override string ToString() => ToString(null, null);

    /// <summary>
    /// Returns the angle in degrees, such as <c>90°</c>.
    /// </summary>
    /// <remarks>
    /// <paramref name="format"/> is a numeric format applied to the value in degrees. For radians, format
    /// <see cref="Radians"/> directly.
    /// </remarks>
    /// <param name="format">The numeric format of the value in degrees.</param>
    /// <param name="formatProvider">The provider of culture-specific formatting.</param>
    /// <returns>The angle in degrees.</returns>
    public string ToString(string? format, IFormatProvider? formatProvider)
        => GeometryFormatting.ToString(this, format, formatProvider);

    /// <inheritdoc/>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (Degrees.TryFormat(destination, out int written, format, provider) && DegreeSign.TryCopyTo(destination[written..]))
        {
            charsWritten = written + DegreeSign.Length;
            return true;
        }

        charsWritten = 0;
        return false;
    }

    /// <inheritdoc/>
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (Degrees.TryFormat(utf8Destination, out int written, format, provider)
            && Encoding.UTF8.TryGetBytes(DegreeSign, utf8Destination[written..], out int sign))
        {
            bytesWritten = written + sign;
            return true;
        }

        bytesWritten = 0;
        return false;
    }

    /// <summary>
    /// Adds two angles.
    /// </summary>
    /// <param name="left">The first angle.</param>
    /// <param name="right">The second angle.</param>
    /// <returns>The sum of the two angles.</returns>
    public static Angle operator +(Angle left, Angle right) => new(left.Radians + right.Radians);

    /// <summary>
    /// Subtracts an angle from another.
    /// </summary>
    /// <param name="left">The angle to subtract from.</param>
    /// <param name="right">The angle to subtract.</param>
    /// <returns>The difference of the two angles.</returns>
    public static Angle operator -(Angle left, Angle right) => new(left.Radians - right.Radians);

    /// <summary>
    /// Negates an angle.
    /// </summary>
    /// <param name="value">The angle to negate.</param>
    /// <returns>The angle turning the other way.</returns>
    public static Angle operator -(Angle value) => new(-value.Radians);

    /// <summary>
    /// Multiplies an angle by a scalar.
    /// </summary>
    /// <param name="left">The angle.</param>
    /// <param name="right">The scalar.</param>
    /// <returns>The scaled angle.</returns>
    public static Angle operator *(Angle left, float right) => new(left.Radians * right);

    /// <summary>
    /// Multiplies an angle by a scalar.
    /// </summary>
    /// <param name="left">The scalar.</param>
    /// <param name="right">The angle.</param>
    /// <returns>The scaled angle.</returns>
    public static Angle operator *(float left, Angle right) => new(left * right.Radians);

    /// <summary>
    /// Divides an angle by a scalar.
    /// </summary>
    /// <remarks>
    /// Follows floating-point rules: dividing by zero gives an infinite or NaN angle, it does not throw.
    /// </remarks>
    /// <param name="left">The angle.</param>
    /// <param name="right">The scalar.</param>
    /// <returns>The divided angle.</returns>
    public static Angle operator /(Angle left, float right) => new(left.Radians / right);

    /// <summary>
    /// Determines whether two angles are exactly equal.
    /// </summary>
    /// <param name="left">The first angle.</param>
    /// <param name="right">The second angle.</param>
    /// <returns><see langword="true"/> if the angles are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Angle left, Angle right) => left.Equals(right);

    /// <summary>
    /// Determines whether two angles differ.
    /// </summary>
    /// <param name="left">The first angle.</param>
    /// <param name="right">The second angle.</param>
    /// <returns><see langword="true"/> if the angles differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Angle left, Angle right) => !left.Equals(right);

    /// <summary>
    /// Determines whether an angle is smaller than another.
    /// </summary>
    /// <param name="left">The first angle.</param>
    /// <param name="right">The second angle.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is smaller; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(Angle left, Angle right) => left.Radians < right.Radians;

    /// <summary>
    /// Determines whether an angle is larger than another.
    /// </summary>
    /// <param name="left">The first angle.</param>
    /// <param name="right">The second angle.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is larger; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(Angle left, Angle right) => left.Radians > right.Radians;

    /// <summary>
    /// Determines whether an angle is smaller than or equal to another.
    /// </summary>
    /// <param name="left">The first angle.</param>
    /// <param name="right">The second angle.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is smaller or equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(Angle left, Angle right) => left.Radians <= right.Radians;

    /// <summary>
    /// Determines whether an angle is larger than or equal to another.
    /// </summary>
    /// <param name="left">The first angle.</param>
    /// <param name="right">The second angle.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is larger or equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(Angle left, Angle right) => left.Radians >= right.Radians;
}
