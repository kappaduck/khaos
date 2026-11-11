// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Diagnostics;

namespace KappaDuck.Khaos.Events;

/// <summary>
/// Groups the game's own events, each a <see langword="readonly record struct"/>, on a
/// <see langword="readonly partial struct"/>, so that <c>OnEvent</c> matches them by name on a <see cref="CustomEvent"/>.
/// </summary>
/// <param name="events">The events of the group, such as <c>typeof(LevelCompleted)</c>.</param>
[AttributeUsage(AttributeTargets.Struct, Inherited = false)]
[Conditional("KHAOS_CUSTOM_EVENTS_METADATA")]
public sealed class CustomEventsAttribute(params Type[] events) : Attribute
{
    /// <summary>
    /// Gets the events of the group.
    /// </summary>
    public IReadOnlyList<Type> Events { get; } = events;
}
