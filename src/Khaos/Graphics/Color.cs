// Copyright (c) KappaDuck.
// Licensed under the MIT license.

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
public readonly partial struct Color(byte r, byte g, byte b, byte a = byte.MaxValue)
{
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
}
