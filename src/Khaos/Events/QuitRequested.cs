// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Events;

/// <summary>
/// The player asked to close the game, with the close button of the window, Alt+F4 or Ctrl+C. The game exits once
/// <c>OnEvent</c> returns.
/// </summary>
[KhaosEvent(SDL_EventType.Quit)]
public readonly record struct QuitRequested;
