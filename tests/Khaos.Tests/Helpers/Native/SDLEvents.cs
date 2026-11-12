// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting.Engine;
using KappaDuck.Khaos.Input;
using KappaDuck.Khaos.Interop.SDL;
using KappaDuck.Khaos.Interop.SDL.Primitives;
using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Tests.Helpers.Native;

internal static partial class SDLEvents
{
    private const int Capacity = 8;
    private const int Get = 2;
    private const int Peek = 1;

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

    internal static int DispatchCustomEvents(KhaosEngine engine)
    {
        Span<SDL_Event> events = stackalloc SDL_Event[Capacity];
        int dispatched = 0;
        int count;

        do
        {
            unsafe
            {
                fixed (SDL_Event* buffer = events)
                    count = PeepEvents(buffer, Capacity, Get, (uint)SDL_EventType.User, (uint)SDL_EventType.Last);
            }

            if (count < 0)
                throw new InvalidOperationException("SDL_PeepEvents could not read the event queue.");

            for (int i = 0; i < count; i++)
                engine.Receive(in events[i]);

            dispatched += count;
        }
        while (count == Capacity);

        return dispatched;
    }

    internal static void PushKeyDown(Scancode code)
    {
        SDL_Event key = new(new SDL_KeyboardEvent(SDL_EventType.KeyDown, (int)code, 0));

        if (!SDL3.PushEvent(ref key))
            throw new InvalidOperationException("SDL_PushEvent refused the key event.");
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
