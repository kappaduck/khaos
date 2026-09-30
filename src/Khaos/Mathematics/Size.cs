// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Numerics;

namespace KappaDuck.Khaos.Mathematics;

/// <summary>
/// Represents a width and a height in floating-point units.
/// </summary>
/// <param name="width">The width.</param>
/// <param name="height">The height.</param>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Size(float width, float height) :
    IMultiplyOperators<Size, float, Size>,
    IDivisionOperators<Size, float, Size>,
    IEqualityOperators<Size, Size, bool>,
    IEquatable<Size>,
    ISpanFormattable,
    IUtf8SpanFormattable
{
    /// <summary>
    /// Gets the size (0, 0).
    /// </summary>
    public static Size Zero { get; }

    /// <summary>
    /// Gets the width.
    /// </summary>
    public float Width { get; init; } = width;

    /// <summary>
    /// Gets the height.
    /// </summary>
    public float Height { get; init; } = height;

    /// <summary>
    /// Gets the area covered by this size.
    /// </summary>
    public float Area => Width * Height;

    /// <summary>
    /// Gets a value indicating whether the width or the height is negative.
    /// </summary>
    public bool IsEmpty => Width < 0f || Height < 0f;

    /// <summary>
    /// Returns the size with each dimension rounded down.
    /// </summary>
    /// <returns>The rounded size.</returns>
    public SizeI Floor() => new((int)MathF.Floor(Width), (int)MathF.Floor(Height));

    /// <summary>
    /// Returns the size with each dimension rounded up.
    /// </summary>
    /// <remarks>
    /// The size to allocate when the content must fit, such as a texture for text measured in floating point.
    /// </remarks>
    /// <returns>The rounded size.</returns>
    public SizeI Ceiling() => new((int)MathF.Ceiling(Width), (int)MathF.Ceiling(Height));

    /// <summary>
    /// Returns the size with each dimension rounded to the nearest integer, halves away from zero.
    /// </summary>
    /// <returns>The rounded size.</returns>
    public SizeI Round()
        => new((int)MathF.Round(Width, MidpointRounding.AwayFromZero), (int)MathF.Round(Height, MidpointRounding.AwayFromZero));

    /// <summary>
    /// Returns the size with each dimension truncated toward zero.
    /// </summary>
    /// <returns>The truncated size.</returns>
    public SizeI Truncate() => new((int)Width, (int)Height);

    /// <summary>
    /// Determines whether this size is within <paramref name="tolerance"/> of another in both dimensions.
    /// </summary>
    /// <param name="other">The size to compare with.</param>
    /// <param name="tolerance">The largest difference allowed on each dimension.</param>
    /// <returns><see langword="true"/> if both dimensions are within <paramref name="tolerance"/>; otherwise, <see langword="false"/>.</returns>
    public bool IsApproximately(Size other, float tolerance)
        => MathF.Abs(Width - other.Width) <= tolerance && MathF.Abs(Height - other.Height) <= tolerance;

    /// <summary>
    /// Deconstructs the size into its dimensions.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public void Deconstruct(out float width, out float height) => (width, height) = (Width, Height);

    /// <inheritdoc/>
    /// <remarks>
    /// The comparison is exact. To compare computed values with a tolerance, use
    /// <see cref="IsApproximately(Size, float)"/>.
    /// </remarks>
#pragma warning disable S1244 // Exact equality is the contract; IsApproximately is the tolerant comparison.
    public bool Equals(Size other) => Width.Equals(other.Width) && Height.Equals(other.Height);
#pragma warning restore S1244

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Size other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Width, Height);

    /// <summary>
    /// Returns the size as <c>(width, height)</c>, using the current culture.
    /// </summary>
    /// <returns>The formatted size.</returns>
    public override string ToString() => ToString(null, null);

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider)
        => GeometryFormatting.ToString(this, format, formatProvider);

    /// <inheritdoc/>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => GeometryFormatting.TryFormat(destination, out charsWritten, [Width, Height], format, provider);

    /// <inheritdoc/>
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => GeometryFormatting.TryFormat(utf8Destination, out bytesWritten, [Width, Height], format, provider);

    /// <summary>
    /// Scales both dimensions.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <param name="scale">The scale factor.</param>
    /// <returns>The scaled size.</returns>
    public static Size operator *(Size size, float scale) => new(size.Width * scale, size.Height * scale);

    /// <summary>
    /// Scales both dimensions.
    /// </summary>
    /// <param name="scale">The scale factor.</param>
    /// <param name="size">The size.</param>
    /// <returns>The scaled size.</returns>
    public static Size operator *(float scale, Size size) => size * scale;

    /// <summary>
    /// Divides both dimensions.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <param name="divisor">The divisor.</param>
    /// <returns>The divided size.</returns>
    public static Size operator /(Size size, float divisor) => new(size.Width / divisor, size.Height / divisor);

    /// <summary>
    /// Determines whether two sizes are exactly equal.
    /// </summary>
    /// <param name="left">The first size.</param>
    /// <param name="right">The second size.</param>
    /// <returns><see langword="true"/> if the sizes are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Size left, Size right) => left.Equals(right);

    /// <summary>
    /// Determines whether two sizes differ.
    /// </summary>
    /// <param name="left">The first size.</param>
    /// <param name="right">The second size.</param>
    /// <returns><see langword="true"/> if the sizes differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Size left, Size right) => !left.Equals(right);

    /// <summary>
    /// Converts a size to the displacement spanning it, width on X and height on Y.
    /// </summary>
    /// <param name="size">The size.</param>
    public static explicit operator Vector2(Size size) => new(size.Width, size.Height);
}
