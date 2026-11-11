// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Events;

internal readonly struct EventContext(float contentScale)
{
    internal float ContentScale { get; } = contentScale;
}
