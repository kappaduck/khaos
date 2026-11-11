// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL.Primitives;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct SDL_WindowEvent(SDL_EventType type, int data1, int data2)
{
    public readonly SDL_EventType Type = type;
    private readonly uint _reserved;
    private readonly ulong _timestamp;
    private readonly uint _windowID;
    public readonly int Data1 = data1;
    public readonly int Data2 = data2;
}
