# Clear the screen

The smallest Khaos program: a window, cleared to a color that changes a little every frame.

<iframe src="/examples/rendering/clear/play/" title="Clear the screen, running in the browser" width="960" height="540" style="width: 100%; height: auto; aspect-ratio: 16 / 9; border: 0;" allow="fullscreen; gamepad; autoplay"></iframe>

What to look at: the whole game is one class. Khaos calls `Initialize` once, which notes the time the game starts, then
calls `Draw` every frame, which clears the screen to a color between teal and crimson that depends on the time since
the start. There is no loop to write: see [How a game runs](../../../guides/game-loop.md). The `#:property Product` line
names the game, and Khaos shows that name in the title bar since the game gives no title of its own.

[!code-csharp[](clear.cs)]
