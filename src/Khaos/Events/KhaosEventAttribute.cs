// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Diagnostics;

namespace KappaDuck.Khaos.Events;

[AttributeUsage(AttributeTargets.Struct, Inherited = false)]
[Conditional("KHAOS_EVENT_METADATA")]
internal sealed class KhaosEventAttribute : Attribute
{
    internal KhaosEventAttribute(SDL_EventType type) : this(type, type)
    {
    }

    internal KhaosEventAttribute(SDL_EventType first, SDL_EventType last)
    {
        First = first;
        Last = last;
    }

    internal SDL_EventType First { get; }

    internal SDL_EventType Last { get; }
}
