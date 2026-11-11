// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Events;

/// <summary>
/// An event the game sent itself with <see cref="EventQueue.TryPush{T}(in T)"/>. Read what it carries with
/// <see cref="TryGet{T}(out T)"/>, inside <c>OnEvent</c>.
/// </summary>
[KhaosEvent(SDL_EventType.User, SDL_EventType.Last)]
public readonly record struct CustomEvent
{
    internal CustomEvent(in SDL_UserEvent native)
    {
        Code = native.Code;
        Type = native.Type;
    }

    internal int Code { get; }

    internal SDL_EventType Type { get; }

    /// <summary>
    /// Tells whether this event carries a <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value the game sent.</typeparam>
    /// <returns><see langword="true"/> if this event carries a <typeparamref name="T"/>; otherwise <see langword="false"/>.</returns>
    public bool Is<T>() where T : struct => CustomEventStore<T>.Owns(Type);

    /// <summary>
    /// Reads the value this event carries when it is a <typeparamref name="T"/>. The value can be read until
    /// <c>OnEvent</c> returns.
    /// </summary>
    /// <typeparam name="T">The type of the value the game sent.</typeparam>
    /// <param name="value">The value when this method returns <see langword="true"/>; otherwise the default value.</param>
    /// <returns><see langword="true"/> if this event carries a <typeparamref name="T"/>; otherwise <see langword="false"/>.</returns>
    public bool TryGet<T>(out T value) where T : struct
    {
        if (CustomEventStore<T>.Slots is { } slots && slots.Type == Type)
            return slots.TryRead(Code, out value);

        value = default;
        return false;
    }

    internal void Release() => CustomEventRegistry.Release(Type, Code);
}
