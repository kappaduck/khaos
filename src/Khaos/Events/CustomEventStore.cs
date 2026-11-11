// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Events;

internal static class CustomEventStore<T> where T : struct
{
    private const int DefaultCapacity = 16;

    internal static CustomEventSlots<T>? Slots { get; set; }

    internal static CustomEventSlots<T> GetOrRegister() => Slots ?? CustomEventRegistry.Register<T>(DefaultCapacity, false);

    internal static bool Owns(SDL_EventType type) => Slots is { } slots && slots.Type == type;

    internal static void Register(int capacity) => _ = CustomEventRegistry.Register<T>(capacity, true);
}
