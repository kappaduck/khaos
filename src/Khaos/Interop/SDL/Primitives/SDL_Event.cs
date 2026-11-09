// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL.Primitives;

[StructLayout(LayoutKind.Explicit, Size = 128)]
internal struct SDL_Event
{
    [FieldOffset(0)]
    public safe SDL_EventType Type;
}
