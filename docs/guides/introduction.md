# What is Khaos?

Khaos is a framework for making games in C#, 2D today and 3D later. You write your game as a C# class; Khaos opens
the window, runs the game, reads the keyboard, mouse and gamepads, draws your sprites, plays your sounds, and does it
the same way on Windows, on Linux and in a web browser.

Under the hood it uses [SDL3](https://www.libsdl.org/), the library behind many commercial games and engines, but you
never see it: there is no C code to call, no pointers, no native setup. You install a NuGet package and write C#.

## Who it is for

- **You know some C# and want to make games.** Khaos is a good first engine: there is no editor to learn and no
  hidden magic, and everything your game does is in code you can read.
- **You are entering a game jam.** A new game runs in one command, and another command makes a zip ready to
  upload to the jam. See [Khaos for a game jam](game-jam.md).
- **You come from another engine.** If you have used Unity, Godot, MonoGame or raylib, most ideas carry over. See
  [Coming from another engine](coming-from.md).
- **You are a .NET developer.** Settings, logging and dependency injection work exactly as in an ASP.NET Core or
  console app.

## A framework, not an editor

Engines such as Unity and Godot are built around an editor: you place things in scenes with the mouse and attach
scripts to them. Khaos has no editor. Your game lives in your code, like with MonoGame or raylib, and you use the
tools you prefer for the rest: an image editor for sprites, a level editor such as Tiled or LDtk for maps.

What Khaos adds around the code is what saves you time:

- `dotnet new khaos` creates a game that already runs, on the desktop and in the browser.
- A missing or misspelled image name is a build error, not a crash in the middle of a level.
- While you work, an overlay shows how fast your game runs; it disappears from the version players download.
- `dotnet publish` makes the file you upload, with a loading screen and a Play button for the web.

## What a game looks like

A game is a class that derives from `Game2D`. It overrides the methods it needs; the smallest game only draws:

```csharp
internal sealed class MyGame() : Game2D("My Khaos Game", 1280, 720)
{
    protected override void Draw(Renderer renderer, in Frame frame)
    {
        renderer.Clear(Color.Slate);   // fill the window with one color
        renderer.Present();            // show what was drawn
    }
}
```

`Program.cs` starts it:

```csharp
return KhaosApplication.CreateBuilder(args)
                       .Build<MyGame>()
                       .Run();
```

That is the whole program. [How a game runs](game-loop.md) explains what happens and the
other methods a game can override: loading, reacting to input, updating, drawing.

## Where it runs

Your game is written once, and players get it in two ways:

- **A download for Windows or Linux**: one executable, with nothing else to install.
- **A page in the browser**: on a jam's page or any website, with nothing to install at all. Mac
  players play this version.

You make the game on **Windows** or **Linux**, in the editor you like: Visual Studio, Rider or VS Code. The template
sets up one app per platform around your game, and each one runs the same code. [Supported platforms](platforms.md)
has the details.

## 2D today, 3D next

Khaos makes 2D games: sprites, tilemaps, cameras, text, sound. That is what the first versions are about, on the way
to [Ludum Dare](https://ludumdare.com/) in April 2027.

3D comes after that. A 3D game will derive from `Game3D` the way a 2D game derives from `Game2D`, with the same
loop, settings and tools around it, on the desktop first. The [roadmap](roadmap.md) says what comes when.

## How it is packaged

A game needs one NuGet package, `KappaDuck.Khaos`. It brings everything else, for every platform.
`KappaDuck.Khaos.Toolkit` adds ready-made pieces (scenes, camera, tilemaps, tweens, collisions) that you can use or
ignore. The template references both.

## Where to go next

1. [Installation](installation.md): what to install, once.
2. [Your first game](getting-started.md): create a game, run it, publish it.
3. [Examples](../examples/readme.md): one feature at a time, running in your browser.

> [!NOTE]
> Khaos is young: names can still change from one version to the next until 1.0. The [roadmap](roadmap.md) says
> what each version brings.
