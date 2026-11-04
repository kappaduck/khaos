// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL.Primitives;

[Flags]
internal enum SDL_WindowStates : ulong
{
    None = 0,
    Fullscreen = 0x0000000000000001,
    Occluded = 0x0000000000000004,
    Hidden = 0x0000000000000008,
    Borderless = 0x0000000000000010,
    Resizable = 0x0000000000000020,
    Minimized = 0x0000000000000040,
    Maximized = 0x0000000000000080,
    InputFocus = 0x0000000000000200,
    HighPixelDensity = 0x0000000000002000,
    NotFocusable = 0x0000000080000000
}
