// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Events;

internal static class CustomEventRegistry
{
    private static readonly Lock _lock = new();

    private static CustomEventSlots[] _slots = [];

    internal static CustomEventSlots<T> Register<T>(int capacity, bool explicitly) where T : struct
    {
        lock (_lock)
        {
            if (CustomEventStore<T>.Slots is { } existing)
            {
                if (explicitly && existing.Capacity != capacity)
                    throw new InvalidOperationException($"{typeof(T).Name} already has {existing.Capacity} slots. Register it once, before its first push.");

                return existing;
            }

            uint type = SDL3.RegisterEvents(1);

            if (type == 0)
                throw new InvalidOperationException("Every type of custom event is taken. A game sends at most 32767 types of its own events.");

            CustomEventSlots<T> slots = new((SDL_EventType)type, capacity);

            _slots = [.. _slots, slots];
            CustomEventStore<T>.Slots = slots;

            return slots;
        }
    }

    internal static void Release(SDL_EventType type, int code)
    {
        foreach (CustomEventSlots slots in _slots)
        {
            if (slots.Type != type)
                continue;

            slots.Release(code);
            return;
        }
    }

    internal static void ReleaseAll()
    {
        foreach (CustomEventSlots slots in _slots)
            slots.ReleaseAll();
    }
}
