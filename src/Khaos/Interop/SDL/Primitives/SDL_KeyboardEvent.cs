// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL.Primitives;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct SDL_KeyboardEvent(SDL_EventType type, int scancode, byte repeat)
{
    public readonly SDL_EventType Type = type;
    private readonly uint _reserved;
    private readonly ulong _timestamp;
    private readonly uint _windowID;
    private readonly uint _which;
    public readonly int Scancode = scancode;
    private readonly uint _key;
    private readonly ushort _mod;
    private readonly ushort _raw;
    private readonly byte _down;
    public readonly byte Repeat = repeat;
}
