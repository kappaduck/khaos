// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL.Primitives;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct SDL_UserEvent(SDL_EventType type, int code)
{
    public readonly SDL_EventType Type = type;
    private readonly uint _reserved;
    private readonly ulong _timestamp;
    private readonly uint _windowID;
    public readonly int Code = code;
    private readonly void* _data1;
    private readonly void* _data2;
}
