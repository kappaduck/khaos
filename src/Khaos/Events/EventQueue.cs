// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Events;

/// <summary>
/// Sends events of the game's own, such as "level finished", from any thread. Each one reaches <c>OnEvent</c> as a
/// <see cref="CustomEvent"/>, before the next frame draws.
/// </summary>
public static class EventQueue
{
    /// <summary>
    /// Sets how many events of type <typeparamref name="T"/> can wait to be handled at once, 16 by default. Call it
    /// before the first push of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values the game sends.</typeparam>
    /// <param name="capacity">How many events can wait at once, from 1 to 65536.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is below 1 or above 65536.</exception>
    /// <exception cref="InvalidOperationException">
    /// <typeparamref name="T"/> was already registered or pushed with another capacity, or the game already uses every type of
    /// custom event.
    /// </exception>
    public static void Register<T>(int capacity) where T : struct
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(capacity, CustomEventSlots.MaxCapacity);

        CustomEventStore<T>.Register(capacity);
    }

    /// <summary>
    /// Sends <paramref name="payload"/> to <c>OnEvent</c>, as a <see cref="CustomEvent"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to send.</typeparam>
    /// <param name="payload">The value to send. It is copied before this method returns.</param>
    /// <returns>
    /// <see langword="true"/> if the event is on its way; <see langword="false"/> when as many events of
    /// <typeparamref name="T"/> as its capacity are already waiting, or when the game is not running.
    /// </returns>
    /// <exception cref="InvalidOperationException">The game already uses every type of custom event.</exception>
    public static bool TryPush<T>(in T payload) where T : struct
    {
        CustomEventSlots<T> slots = CustomEventStore<T>.GetOrRegister();

        if (!slots.TryAdd(in payload, out int code))
            return false;

        SDL_Event native = new(new SDL_UserEvent(slots.Type, code));

        if (SDL3.PushEvent(ref native))
            return true;

        slots.Release(code);
        return false;
    }
}
