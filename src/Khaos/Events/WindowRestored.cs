// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Events;

/// <summary>
/// The window came back to its normal size and position, after being minimized or maximized.
/// </summary>
[KhaosEvent(SDL_EventType.WindowRestored)]
public readonly record struct WindowRestored;
