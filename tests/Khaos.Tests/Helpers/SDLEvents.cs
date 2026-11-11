// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.SDL;
using KappaDuck.Khaos.Interop.SDL.Primitives;
using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Tests.Helpers;

internal static partial class SDLEvents
{
    private const int Peek = 1;
    private const int Capacity = 8;

    internal static int CountQuit()
    {
        Span<SDL_Event> events = stackalloc SDL_Event[Capacity];
        int count;

        unsafe
        {
            fixed (SDL_Event* buffer = events)
                count = PeepEvents(buffer, Capacity, Peek, (uint)SDL_EventType.Quit, (uint)SDL_EventType.Quit);
        }

        if (count < 0)
            throw new InvalidOperationException("SDL_PeepEvents could not read the event queue.");

        return count;
    }

    internal static void PushQuit()
    {
        SDL_Event quit = new(SDL_EventType.Quit);

        if (!SDL3.PushEvent(ref quit))
            throw new InvalidOperationException("SDL_PushEvent refused the quit event.");
    }

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_PeepEvents")]
    private static unsafe partial int PeepEvents(SDL_Event* events, int count, int action, uint minType, uint maxType);
}
