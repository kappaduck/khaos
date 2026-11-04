// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL.Primitives;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct SDL_DisplayMode
{
    private readonly uint _displayID;
    private readonly SDL_PixelFormat _format;
    private readonly int _width;
    private readonly int _height;
    public readonly float PixelDensity;
    public readonly float RefreshRate;
    private readonly int _refreshRateNumerator;
    private readonly int _refreshRateDenominator;
    private readonly void* _internal;
}
