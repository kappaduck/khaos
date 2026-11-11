// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Events;

/// <summary>
/// The window got the keyboard focus, so the keys the player presses now reach the game.
/// </summary>
[KhaosEvent(SDL_EventType.WindowFocusGained)]
public readonly record struct WindowFocusGained;
