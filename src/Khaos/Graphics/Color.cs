// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;
using System.Buffers;
using System.Numerics;
using System.Text.Unicode;

namespace KappaDuck.Khaos.Graphics;

/// <summary>
/// Represents a color by its red, green, blue and alpha channels, each from 0 to 255.
/// </summary>
/// <remarks>
/// <para>
/// Alpha is the opacity, from 0 (fully transparent) to 255 (fully opaque). A color you create without it is opaque.
/// </para>
/// <para>
/// The named colors, such as <see cref="Teal"/>, are the colors of CSS, with the same names and values as on
/// the web.
/// </para>
/// </remarks>
/// <param name="r">The red channel, from 0 to 255.</param>
/// <param name="g">The green channel, from 0 to 255.</param>
/// <param name="b">The blue channel, from 0 to 255.</param>
/// <param name="a">The alpha channel, from 0 (transparent) to 255 (opaque). 255 by default.</param>
[StructLayout(LayoutKind.Sequential)]
public readonly partial struct Color(byte r, byte g, byte b, byte a = byte.MaxValue) :
    IEqualityOperators<Color, Color, bool>,
    IEquatable<Color>,
    ISpanFormattable,
    IUtf8SpanFormattable
{
    private const int ChannelCount = 4;
    private const int LongHexDigits = ChannelCount * 2;
    private const int ShortRgbDigits = 3;
    private const int ShortRgbaDigits = 4;
    private const int RgbDigits = 6;

    /// <summary>
    /// Gets the red channel, from 0 to 255.
    /// </summary>
    public byte R { get; init; } = r;

    /// <summary>
    /// Gets the green channel, from 0 to 255.
    /// </summary>
    public byte G { get; init; } = g;

    /// <summary>
    /// Gets the blue channel, from 0 to 255.
    /// </summary>
    public byte B { get; init; } = b;

    /// <summary>
    /// Gets the alpha channel, from 0 (transparent) to 255 (opaque).
    /// </summary>
    public byte A { get; init; } = a;

    /// <summary>
    /// Gets a fully transparent color, with every channel at 0.
    /// </summary>
    public static Color Transparent => default;

    /// <summary>
    /// Deconstructs the color into its channels.
    /// </summary>
    /// <param name="r">The red channel.</param>
    /// <param name="g">The green channel.</param>
    /// <param name="b">The blue channel.</param>
    /// <param name="a">The alpha channel.</param>
    public void Deconstruct(out byte r, out byte g, out byte b, out byte a) => (r, g, b, a) = (R, G, B, A);

    /// <summary>
    /// Packs the color into a number written <c>0xRRGGBBAA</c>.
    /// </summary>
    /// <returns>The packed color, red in the highest byte and alpha in the lowest.</returns>
    public uint ToHex() => ((uint)R << 24) | ((uint)G << 16) | ((uint)B << 8) | A;

    /// <summary>
    /// Creates a color from a number written <c>0xRRGGBBAA</c>.
    /// </summary>
    /// <remarks>
    /// The alpha is part of the number, so <c>0x6495ED</c> is a transparent color. Write <c>0x6495EDFF</c> for an opaque
    /// one.
    /// </remarks>
    /// <param name="value">The packed color, red in the highest byte and alpha in the lowest.</param>
    /// <returns>The color.</returns>
    public static Color FromHex(uint value) => new((byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value);

    /// <summary>
    /// Creates a color from hexadecimal text such as <c>#6495ED</c>, as written in CSS or in a paint program.
    /// </summary>
    /// <remarks>
    /// The <c>#</c> is optional, and so is the case. It accepts <c>RGB</c>, <c>RGBA</c>, <c>RRGGBB</c> and
    /// <c>RRGGBBAA</c>, where each letter is one hexadecimal digit. A short layout doubles each digit, so <c>#ABC</c> is
    /// <c>#AABBCC</c>, and a color without alpha is opaque.
    /// </remarks>
    /// <param name="hex">The hexadecimal text.</param>
    /// <returns>The color.</returns>
    /// <exception cref="FormatException"><paramref name="hex"/> is not a hexadecimal color.</exception>
    public static Color FromHex(ReadOnlySpan<char> hex)
    {
        if (!TryFromHex(hex, out Color color))
            throw new FormatException($"'{hex}' is not a hexadecimal color such as #RGB, #RGBA, #RRGGBB or #RRGGBBAA.");

        return color;
    }

    /// <summary>
    /// Tries to create a color from hexadecimal text such as <c>#6495ED</c>, as written in CSS or in a paint program.
    /// </summary>
    /// <remarks>
    /// Accepts the same layouts as <see cref="FromHex(ReadOnlySpan{char})"/>.
    /// </remarks>
    /// <param name="hex">The hexadecimal text.</param>
    /// <param name="color">The color, or <see cref="Transparent"/> when <paramref name="hex"/> is not a hexadecimal color.</param>
    /// <returns><see langword="true"/> when <paramref name="hex"/> is a hexadecimal color; otherwise, <see langword="false"/>.</returns>
    public static bool TryFromHex(ReadOnlySpan<char> hex, out Color color)
    {
        color = default;
        hex = hex.Trim();

        if (hex.StartsWith('#'))
            hex = hex[1..];

        Span<char> digits = stackalloc char[LongHexDigits];

        if (!TryWriteLongLayout(hex, digits, out int count))
            return false;

        Span<byte> channels = stackalloc byte[ChannelCount];
        channels[^1] = byte.MaxValue;

        if (Convert.FromHexString(digits[..count], channels, out _, out _) != OperationStatus.Done)
            return false;

        color = new Color(channels[0], channels[1], channels[2], channels[3]);
        return true;
    }

    /// <summary>
    /// Blends two colors, channel by channel, alpha included.
    /// </summary>
    /// <remarks>
    /// Use it to fade a sprite out, to flash a hit, or to move a sky from day to night over a few seconds.
    /// </remarks>
    /// <param name="from">The color at 0.</param>
    /// <param name="to">The color at 1.</param>
    /// <param name="factor">How far to go from <paramref name="from"/> to <paramref name="to"/>, kept between 0 and 1.</param>
    /// <returns>The blended color, each channel rounded to the nearest whole number, halves up.</returns>
    public static Color Lerp(Color from, Color to, float factor)
    {
        factor = Math.Clamp(factor, 0f, 1f);

        return new Color(
            Blend(from.R, to.R, factor),
            Blend(from.G, to.G, factor),
            Blend(from.B, to.B, factor),
            Blend(from.A, to.A, factor));
    }

    /// <inheritdoc/>
    public bool Equals(Color other) => R == other.R && G == other.G && B == other.B && A == other.A;

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Color other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(R, G, B, A);

    /// <summary>
    /// Returns the color as <c>(r, g, b, a)</c>, using the current culture.
    /// </summary>
    /// <returns>The formatted color.</returns>
    public override string ToString() => ToString(null, null);

    /// <summary>
    /// Returns the color as <c>(r, g, b, a)</c>, or as <c>#RRGGBBAA</c> with the <c>X</c> format (<c>x</c> for lowercase).
    /// </summary>
    /// <param name="format">
    /// <c>X</c> or <c>x</c> for hexadecimal text; otherwise, a numeric format applied to each channel.
    /// </param>
    /// <param name="formatProvider">The culture, or <see langword="null"/> for the current culture.</param>
    /// <returns>The formatted color.</returns>
    public string ToString(string? format, IFormatProvider? formatProvider)
        => GeometryFormatter.ToString(this, format, formatProvider);

    /// <inheritdoc/>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (format is "X")
            return destination.TryWrite($"#{R:X2}{G:X2}{B:X2}{A:X2}", out charsWritten);

        if (format is "x")
            return destination.TryWrite($"#{R:x2}{G:x2}{B:x2}{A:x2}", out charsWritten);

        return GeometryFormatter.TryFormat(destination, out charsWritten, [R, G, B, A], format, provider);
    }

    /// <inheritdoc/>
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (format is "X")
            return Utf8.TryWrite(utf8Destination, $"#{R:X2}{G:X2}{B:X2}{A:X2}", out bytesWritten);

        if (format is "x")
            return Utf8.TryWrite(utf8Destination, $"#{R:x2}{G:x2}{B:x2}{A:x2}", out bytesWritten);

        return GeometryFormatter.TryFormat(utf8Destination, out bytesWritten, [R, G, B, A], format, provider);
    }

    /// <summary>
    /// Determines whether two colors are equal.
    /// </summary>
    /// <param name="left">The first color.</param>
    /// <param name="right">The second color.</param>
    /// <returns><see langword="true"/> if every channel is equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Color left, Color right) => left.Equals(right);

    /// <summary>
    /// Determines whether two colors differ.
    /// </summary>
    /// <param name="left">The first color.</param>
    /// <param name="right">The second color.</param>
    /// <returns><see langword="true"/> if a channel differs; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Color left, Color right) => !left.Equals(right);

    private static byte Blend(byte from, byte to, float factor)
        => (byte)MathF.Round(from + ((to - from) * factor), MidpointRounding.AwayFromZero);

    private static bool TryWriteLongLayout(ReadOnlySpan<char> hex, Span<char> digits, out int count)
    {
        switch (hex.Length)
        {
            case ShortRgbDigits or ShortRgbaDigits:
                for (int i = 0; i < hex.Length; i++)
                    digits[2 * i] = digits[(2 * i) + 1] = hex[i];

                count = hex.Length * 2;
                return true;

            case RgbDigits or LongHexDigits:
                hex.CopyTo(digits);
                count = hex.Length;
                return true;

            default:
                count = 0;
                return false;
        }
    }
}
