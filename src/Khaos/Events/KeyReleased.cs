// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Input;

namespace KappaDuck.Khaos.Events;

/// <summary>
/// A key went up.
/// </summary>
/// <param name="Code">The position of the key on the keyboard, whatever its layout.</param>
[KhaosEvent(SDL_EventType.KeyUp)]
public readonly record struct KeyReleased(Scancode Code)
{
    internal KeyReleased(in SDL_KeyboardEvent native) : this((Scancode)native.Scancode)
    {
    }
}
