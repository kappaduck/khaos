// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;
using KappaDuck.Khaos.Hosting;
using KappaDuck.Khaos.Input;
using KappaDuck.Khaos.Interop.SDL;
using KappaDuck.Khaos.Interop.SDL.Primitives;
using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Benchmarks.Frame;

internal static partial class QueuedEvents
{
    private const int Capacity = 64;
    private const uint FirstType = 0;
    private const int GetEvent = 2;

    internal static void Dispatch(KhaosEngine engine)
    {
        Span<SDL_Event> events = stackalloc SDL_Event[Capacity];
        int count;

        do
        {
            unsafe
            {
                fixed (SDL_Event* buffer = events)
                    count = PeepEvents(buffer, Capacity, GetEvent, FirstType, (uint)SDL_EventType.Last);
            }

            if (count < 0)
                throw new InvalidOperationException("SDL_PeepEvents could not read the event queue.");

            for (int i = 0; i < count; i++)
                engine.Receive(in events[i]);
        }
        while (count == Capacity);
    }

    internal static void Push(int count, int frame)
    {
        if (count == 0)
            return;

        Push(new SDL_Event(new SDL_WindowEvent(SDL_EventType.WindowResized, 1280, 720)));
        _ = EventQueue.TryPush(new ScoreChanged(frame));

        for (int i = 2; i < count; i++)
        {
            SDL_EventType type = i % 2 == 0 ? SDL_EventType.KeyDown : SDL_EventType.KeyUp;
            Push(new SDL_Event(new SDL_KeyboardEvent(type, (int)Scancode.Space, repeat: 0)));
        }
    }

    private static void Push(SDL_Event e)
    {
        if (!SDL3.PushEvent(ref e))
            throw new InvalidOperationException("SDL_PushEvent refused the event.");
    }

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_PeepEvents")]
    private static unsafe partial int PeepEvents(SDL_Event* events, int count, int action, uint minType, uint maxType);
}
