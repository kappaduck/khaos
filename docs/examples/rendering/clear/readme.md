# Clear the screen

The smallest Khaos program: a window, cleared to one color every frame.

<iframe src="/examples/rendering/clear/play/" title="Clear the screen, running in the browser" width="960" height="540" style="width: 100%; height: auto; aspect-ratio: 16 / 9; border: 0;" allow="fullscreen; gamepad; autoplay"></iframe>

What to look at: the whole game is one class with a single `Draw` method, which Khaos calls every frame. There is no
loop to write: see [How a game runs](../../../guides/game-loop.md).
