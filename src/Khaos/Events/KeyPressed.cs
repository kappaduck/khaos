// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Input;

namespace KappaDuck.Khaos.Events;

/// <summary>
/// A key went down. It arrives once per press, and holding the key does not repeat it.
/// </summary>
/// <param name="Code">The position of the key on the keyboard, whatever its layout.</param>
[KhaosEvent(SDL_EventType.KeyDown)]
public readonly record struct KeyPressed(Scancode Code)
{
    internal KeyPressed(in SDL_KeyboardEvent native) : this((Scancode)native.Scancode)
    {
    }
}
