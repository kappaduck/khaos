# Keys

Hold R, G or B to color the screen red, green or blue. Let go and it turns back to teal. Escape closes the game.

<iframe src="/examples/events/keys/play/?click" title="Keys, running in the browser" width="960" height="540" style="width: 100%; height: auto; aspect-ratio: 16 / 9; border: 0;" allow="fullscreen; gamepad; autoplay"></iframe>

What to look at: Khaos calls `OnEvent` once for each key the player presses or releases, before the frame is drawn.
The `switch` checks which event it is with a property pattern, such as `KeyPressed { Code: Scancode.R }`, and
`Draw` clears the screen to the color the last event chose. `KeyPressed` comes once per press, even when the key is
held, so a key held down never repeats the action.

`Scancode` names a key by its place on the keyboard, not by the letter printed on it. On a French AZERTY keyboard,
`Scancode.A` is the key labeled Q, which keeps the controls of a game in the same place for every player. See
[Events](../../../guides/events.md) for every event and how to send your own.

[!code-csharp[](keys.cs)]
