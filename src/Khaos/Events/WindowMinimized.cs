// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Events;

/// <summary>
/// The window was minimized to the taskbar. <see cref="WindowRestored"/> follows when it comes back.
/// </summary>
[KhaosEvent(SDL_EventType.WindowMinimized)]
public readonly record struct WindowMinimized;
