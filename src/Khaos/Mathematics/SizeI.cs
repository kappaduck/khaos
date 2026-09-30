// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Numerics;

namespace KappaDuck.Khaos.Mathematics;

/// <summary>
/// Represents a width and a height in whole units, such as a window, a texture or a grid.
/// </summary>
/// <param name="width">The width.</param>
/// <param name="height">The height.</param>
[StructLayout(LayoutKind.Sequential)]
public readonly struct SizeI(int width, int height) :
    IMultiplyOperators<SizeI, int, SizeI>,
    IMultiplyOperators<SizeI, float, Size>,
    IEqualityOperators<SizeI, SizeI, bool>,
    IEquatable<SizeI>,
    ISpanFormattable,
    IUtf8SpanFormattable
{
    /// <summary>
    /// Gets the size (0, 0).
    /// </summary>
    public static SizeI Zero { get; }

    /// <summary>
    /// Gets the width.
    /// </summary>
    public int Width { get; init; } = width;

    /// <summary>
    /// Gets the height.
    /// </summary>
    public int Height { get; init; } = height;

    /// <summary>
    /// Gets the area covered by this size.
    /// </summary>
    /// <remarks>
    /// A <see cref="long"/>, because two dimensions above 46,340 overflow an <see cref="int"/>.
    /// </remarks>
    public long Area => (long)Width * Height;

    /// <summary>
    /// Gets a value indicating whether the width or the height is zero or negative.
    /// </summary>
    public bool IsEmpty => Width <= 0 || Height <= 0;

    /// <summary>
    /// Deconstructs the size into its dimensions.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public void Deconstruct(out int width, out int height) => (width, height) = (Width, Height);

    /// <inheritdoc/>
    public bool Equals(SizeI other) => Width == other.Width && Height == other.Height;

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is SizeI other && Equals(other);

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
    /// Scales both dimensions by a whole factor.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <param name="scale">The scale factor.</param>
    /// <returns>The scaled size.</returns>
    public static SizeI operator *(SizeI size, int scale) => new(size.Width * scale, size.Height * scale);

    /// <summary>
    /// Scales both dimensions by a whole factor.
    /// </summary>
    /// <param name="scale">The scale factor.</param>
    /// <param name="size">The size.</param>
    /// <returns>The scaled size.</returns>
    public static SizeI operator *(int scale, SizeI size) => size * scale;

    /// <summary>
    /// Scales both dimensions by a fractional factor, giving a floating-point size.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <param name="scale">The scale factor.</param>
    /// <returns>The scaled size.</returns>
    public static Size operator *(SizeI size, float scale) => new(size.Width * scale, size.Height * scale);

    /// <summary>
    /// Scales both dimensions by a fractional factor, giving a floating-point size.
    /// </summary>
    /// <param name="scale">The scale factor.</param>
    /// <param name="size">The size.</param>
    /// <returns>The scaled size.</returns>
    public static Size operator *(float scale, SizeI size) => size * scale;

    /// <summary>
    /// Determines whether two sizes are equal.
    /// </summary>
    /// <param name="left">The first size.</param>
    /// <param name="right">The second size.</param>
    /// <returns><see langword="true"/> if the sizes are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(SizeI left, SizeI right) => left.Equals(right);

    /// <summary>
    /// Determines whether two sizes differ.
    /// </summary>
    /// <param name="left">The first size.</param>
    /// <param name="right">The second size.</param>
    /// <returns><see langword="true"/> if the sizes differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(SizeI left, SizeI right) => !left.Equals(right);

    /// <summary>
    /// Converts an integer size to a floating-point size.
    /// </summary>
    /// <param name="size">The size to convert.</param>
    public static implicit operator Size(SizeI size) => new(size.Width, size.Height);
}
