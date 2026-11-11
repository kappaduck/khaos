// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL.Primitives;

internal enum SDL_EventType : uint
{
    Quit = 0x100,
    WindowResized = 0x206,
    WindowPixelSizeChanged = 0x207,
    WindowMinimized = 0x209,
    WindowRestored = 0x20B,
    WindowFocusGained = 0x20E,
    WindowFocusLost = 0x20F,
    WindowDisplayScaleChanged = 0x214,
    KeyDown = 0x300,
    KeyUp = 0x301,
    User = 0x8000,
    Last = 0xFFFF
}
