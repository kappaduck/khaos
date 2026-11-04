// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;

namespace KappaDuck.Khaos.Windowing;

internal static class WindowScale
{
    internal static float ContentScale(float displayScale, int nativeWidth, int pixelWidth)
        => pixelWidth > 0 ? displayScale * nativeWidth / pixelWidth : displayScale;

    internal static SizeI ToLogical(SizeI native, float contentScale) => ((Size)native / contentScale).Round();

    internal static PointI ToLogical(PointI native, float contentScale)
        => new Point(native.X / contentScale, native.Y / contentScale).Round();

    internal static RectI ToLogical(RectI native, float contentScale)
        => new(ToLogical(native.Position, contentScale), ToLogical(native.Size, contentScale));

    internal static SizeI ToNative(SizeI logical, float contentScale) => (logical * contentScale).Round();

    internal static PointI ToNative(PointI logical, float contentScale)
        => new Point(logical.X * contentScale, logical.Y * contentScale).Round();

    internal static SizeI ToPixels(SizeI native, float pixelDensity) => (native * pixelDensity).Round();

    internal static RectI ToPixels(RectI native, float pixelDensity)
        => new(ToNative(native.Position, pixelDensity), ToPixels(native.Size, pixelDensity));
}
