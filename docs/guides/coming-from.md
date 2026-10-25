# Coming from another engine

If you have made games with Unity, Godot, MonoGame or raylib, most of what you know carries over to Khaos. This page
shows where the familiar pieces are, and the few things that work differently.

## The big difference: no editor

Khaos is closest to **MonoGame** and **raylib**: your game is code, from the first line to the last. There is no
editor, no scene file and no inspector as in **Unity** or **Godot**. You place things by code, and use separate tools
for the rest: an image editor for sprites, Tiled or LDtk for levels.

In exchange, everything about your game is in plain C# files that you can read, search, review and keep in Git.

## Where things are

| You know              | In Khaos                                     | Unity                     | Godot                 | MonoGame                   | raylib                        |
| :-------------------- | :------------------------------------------- | :------------------------ | :-------------------- | :------------------------- | :---------------------------- |
| Where the game starts | Your `Game2D` class, started by `Program.cs` | The first scene           | The main scene        | `Game` class               | `main` with a `while` loop    |
| Loading content       | `Load()`                                     | `Start`, `Awake`          | `_ready`              | `LoadContent`              | Before the loop               |
| Every frame           | `Update()`                                   | `Update`                  | `_process`            | `Update`                   | Inside the loop               |
| Fixed-rate gameplay   | `FixedUpdate()`                              | `FixedUpdate`             | `_physics_process`    | Fixed time step option     | By hand                       |
| Drawing               | `Draw()`                                     | Automatic, from the scene | Automatic, or `_draw` | `Draw` with `SpriteBatch`  | `BeginDrawing` … `EndDrawing` |
| Input                 | `OnEvent()` and input actions                | Input System actions      | Input map             | `Keyboard.GetState()`      | `IsKeyDown`                   |
| Content files         | The `assets/` folder                         | `Assets/` folder          | `res://`              | Content Pipeline (`.mgcb`) | Paths to files                |
| Scenes                | Toolkit scenes, in code                      | Scenes                    | Scenes                | By hand                    | By hand                       |
| Web export            | `dotnet publish`                             | WebGL build               | Web export            | Not built in               | Emscripten build              |

## Things that work differently

**There is no loop to write.** If you come from raylib or a hand-written MonoGame loop: Khaos runs the loop and calls
your methods. That is what lets the same game run in the browser, which does not allow a program to loop forever.
See [How a game runs](game-loop.md).

**Gameplay and drawing are separate.** Like Unity's `FixedUpdate`, Khaos runs gameplay at a fixed rate, zero, one or
several times per frame. Drawing works differently: Unity draws the scene for you after `Update` and `LateUpdate`,
and the few calls that draw by hand, such as `Graphics.RenderMesh`, only queue something for the current frame. In
Khaos you draw everything yourself, in `Draw`, the only method that receives the renderer.

**Coordinates are in pixels, with Y going down.** The top-left corner of the window is (0, 0), as in Godot 2D,
MonoGame and raylib. If you come from Unity, Y is flipped from what you are used to.

**No content pipeline to run.** Coming from MonoGame: there is no `.mgcb` content project, no MGCB Editor and no
`.xnb` files. Drop a PNG in `assets/` and load it: Khaos reads the common formats (PNG, WAV, Ogg, TrueType…) as they
are, and the [asset catalog](assets.md#load-them-by-name-checked-by-the-compiler) still catches a wrong file name at
build time.

**Objects are plain C#.** There are no components to attach and no node tree. A player is a class with fields, held
by your game or by a scene. Organize it however you like.

**Physics is simple, on purpose.** The Toolkit has collisions for rectangles, circles and grids, enough for most 2D
games. A full physics engine is planned after April 2027.

**C# and .NET you already know.** If you come from Unity, Khaos runs on the latest .NET: modern C#, the regular
NuGet packages, and `appsettings.json`, logging and dependency injection as in any .NET app. See
[Configuration and environments](configuration.md).

## Where to go next

- [Your first game](getting-started.md): create one and run it in five minutes.
- [Examples](../examples/readme.md): each feature on its own, with its code.
- [API reference](../api/index.md): look up a class you need.
