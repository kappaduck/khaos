# API reference

Every class, method and property you can use in a Khaos game, with what it does and how to call it. Pick a
namespace in the menu, or search for a name at the top of the page.

The guides explain how the pieces fit together; this reference is where you look up the details once you know what
you want.

## Where things are

| Namespace                            | What you find there                                                        | Start with         |
| :----------------------------------- | :------------------------------------------------------------------------- | :----------------- |
| `KappaDuck.Khaos`                    | Your game class and its loop: what runs each frame, and the time it gets   | `Game2D`           |
| `KappaDuck.Khaos.Hosting`            | Starting the game from `Program.cs`: services, settings, logs              | `KhaosApplication` |
| `KappaDuck.Khaos.Events`             | What happened since the last frame: keys, mouse, gamepads, the window      | `Event`            |
| `KappaDuck.Khaos.Input`              | The keyboard, the mouse, gamepads, and actions such as "jump"              |                    |
| `KappaDuck.Khaos.Graphics`           | Colors, sprites, shapes and animations                                     | `Sprite`           |
| `KappaDuck.Khaos.Graphics.Rendering` | Drawing on the screen: the renderer, textures                              | `Renderer`         |
| `KappaDuck.Khaos.Audio`              | Sound effects, music, volume                                               |                    |
| `KappaDuck.Khaos.Fonts`              | Fonts and text on the screen                                               |                    |
| `KappaDuck.Khaos.Assets`             | Loading your images, sounds and fonts                                      | `Assets`           |
| `KappaDuck.Khaos.Mathematics`        | Positions, sizes, rectangles, angles                                       | `Angle`            |
| `KappaDuck.Khaos.Toolkit.*`          | Ready-made pieces: scenes, camera, tilemaps, particles, tweens, collisions |                    |

You never need a `using` line for the Khaos namespaces in a game made with the template: they are already
available in every file.

> [!NOTE]
> Khaos is young: names can still change from one version to the next until 1.0, and some namespaces above are
> still being written. The [roadmap](../guides/roadmap.md) says what each version brings.
