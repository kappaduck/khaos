# Roadmap

Khaos is growing toward one goal: making a complete 2D game, playable in the browser, for
[Ludum Dare](https://ludumdare.com/) in April 2027. Each version below adds what that takes, and each one runs on
Windows, Linux and in the browser.

| Version | Expected        | What you can do with it                                                                                                                       |
| :-----: | :-------------- | :-------------------------------------------------------------------------------------------------------------------------------------------- |
| `0.1.0` | Late Nov. 2026  | **The foundations**: a window, the game loop with steady timing, events, settings and the `dotnet new khaos` template                         |
| `0.2.0` | Late Dec. 2026  | **Draw and move**: fast 2D drawing, shapes, images and sprites, scaling to any window size, keyboard, mouse, gamepads and input actions       |
| `0.3.0` | Mid Jan. 2027   | **A whole game**: music and sound effects with volume groups, text, assets checked by the compiler, scenes and transitions, timers and tweens |
| `0.4.0` | Early Feb. 2027 | **Bigger worlds**: a 2D camera, sprite sheets and animations, collisions, tilemaps                                                            |
| `0.5.0` | Late Feb. 2027  | **Polish for jams**: splash and loading screens, the debug overlay, particles, simple menus and buttons, render targets                       |
| `0.6.0` | Mar. 31, 2027   | **Ready for the jam**: fixes only, then the engine stays stable through the jam                                                               |

Scenes, the camera, tilemaps, collisions and tweens come in a second package, `KappaDuck.Khaos.Toolkit`, which the
template already includes. You can use them, or write your own.

A feature is finished when its [example](../examples/readme.md) runs in the browser, so the examples are the best
place to see what is ready.

## After the jam

- 3D with `Game3D`, next to `Game2D` and with the same loop and tools: on Windows and Linux first, then in the browser
- Shader effects for 2D games
- Android
- Networking, a physics engine, and more

Dates can move: the jam comes first. What changed in each version is in the
[changelog](https://github.com/kappaduck/khaos/blob/main/changelog.md).
