# Game events

The screen pulses gray while the levels load. After two seconds it turns teal, and each press of Space finishes the
level and moves to the next color. Escape closes the game.

<iframe src="/examples/events/game-events/play/?click" title="Game events, running in the browser" width="960" height="540" style="width: 100%; height: auto; aspect-ratio: 16 / 9; border: 0;" allow="fullscreen; gamepad; autoplay"></iframe>

What to look at: the game declares its own events, `LoadingFinished` and `LevelFinished`, as `readonly record struct`
types, and lists them in `[CustomEvents]` on `GameEvents`. Any code sends one with `EventQueue.TryPush`, even from
another thread, such as the task started in `Initialize` that pretends to load the levels. Khaos hands each of them to
`OnEvent` as a `CustomEvent`, on the game's thread, and `CustomEvent { GameEvents: LevelFinished finished }` matches
it by name.

In a real game, the code that notices the end of a level (the player reaching the exit) is far from the code that
reacts to it (the score, the music, the next level). An event connects the two without either one knowing about
the other. See [Events](../../../guides/events.md) for how long an event lives and what it can hold.

[!code-csharp[](game-events.cs)]
