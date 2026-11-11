// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL.Primitives;

[StructLayout(LayoutKind.Explicit, Size = 128)]
internal readonly struct SDL_Event
{
    internal SDL_Event(SDL_EventType type) => Type = type;

    internal SDL_Event(SDL_WindowEvent window) => Window = window;

    internal SDL_Event(SDL_KeyboardEvent key) => Key = key;

    internal SDL_Event(SDL_UserEvent user) => User = user;

    [FieldOffset(0)]
    public readonly safe SDL_EventType Type;

    [FieldOffset(0)]
    public readonly safe SDL_WindowEvent Window;

    [FieldOffset(0)]
    public readonly safe SDL_KeyboardEvent Key;

    [FieldOffset(0)]
    public readonly safe SDL_UserEvent User;
}
