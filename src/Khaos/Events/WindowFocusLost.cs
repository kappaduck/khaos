// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Events;

/// <summary>
/// The window lost the keyboard focus, for example when the player switched to another app. A good moment to pause.
/// </summary>
[KhaosEvent(SDL_EventType.WindowFocusLost)]
public readonly record struct WindowFocusLost;
