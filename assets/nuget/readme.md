# Khaos

Make chaos. Khaos keeps the order.

Khaos is a .NET 11 game framework built on [SDL3](https://www.libsdl.org/) and its extensions
([SDL_image](https://github.com/libsdl-org/SDL_image), [SDL_mixer](https://github.com/libsdl-org/SDL_mixer),
[SDL_ttf](https://github.com/libsdl-org/SDL_ttf)). It wraps SDL in a clean, idiomatic C# API, and takes care of the
window, the loop and the platform. `Game2D` draws with [SDL_Renderer](https://wiki.libsdl.org/SDL3/CategoryRender).

## Why Khaos

Your game can be chaos, with enemies everywhere and explosions on every side. Khaos keeps the order under it.

- **Your game is a .NET app.** It is plain C#, with no pointers, no native handles and no C-style calls. Dependency
  injection, logging and configuration come from `Microsoft.Extensions`, so what you know from ASP.NET Core works here.
- **The compiler is on your team.** `Event` is a source-generated union, so `OnEvent` pattern-matches on cases like
  `KeyPressed` without boxing or allocating, and a game declares its own events with `[CustomEvents]`. Input actions
  name what the player does (`Jump`, `Move`) and bind it to keys, mouse buttons and gamepads.
  [Analyzers](https://khaos.kappaduck.com/rules/readme.html) report a mistake while you type, and each rule has a page
  that explains why.
- **Every frame counts.** The framework avoids allocating on every frame, so the garbage collector stays out of the
  way, and a project of allocation tests checks it. The simulation runs on a fixed step, drawing smooths the motion
  between two steps, and the [performance](https://khaos.kappaduck.com/performance/) is measured on every release.
- **One game, every platform.** `dotnet new khaos` sets up the game with a desktop and a web application. The
  `assets/` folder goes along, so the game loads a file by the same path on Windows, Linux and in the browser.

> [!NOTE]
> Khaos is in early development and the public API is still a prototype. Expect breaking changes.

## Getting started

```bash
dotnet new install KappaDuck.Khaos.Templates
dotnet new khaos -o Pong
```

Or add the framework to an existing project:

```bash
dotnet package add KappaDuck.Khaos
```

## Usage

```csharp
// Pong/PongGame.cs: the game, a library shared by the desktop and web applications
namespace Pong;

public sealed class PongGame() : Game2D("Pong!", 1280, 720)
{
    private Sprite _ball = null!;
    private Point _previous = new(640f, 360f);
    private Point _position = new(640f, 360f);
    private Vector2 _velocity = new(240f, 180f);

    protected override void Initialize() => _ball = new Sprite(Assets.LoadTexture("assets/sprites/ball.png"));

    protected override void OnEvent(in Event e)
    {
        if (e is KeyPressed { Code: Scancode.Escape })
            Exit();
    }

    protected override void FixedUpdate(in GameTime time)
    {
        _previous = _position;
        _position += _velocity * time.Delta;
    }

    protected override void Update(in GameTime time)
    {
        // Cosmetic only, so it follows the real clock and stays smooth whatever the simulation rate.
        _ball.Rotation += Angle.FromDegrees(180f * time.Delta);
    }

    protected override void Draw(Renderer renderer, in Frame frame)
    {
        _ball.Position = Point.Lerp(_previous, _position, frame.Alpha);

        renderer.Clear(Color.Teal);
        renderer.Draw(_ball);
    }
}
```

```csharp
// Pong.Desktop/Program.cs: configure any settings, then run the game. Pong.Web has its own too.
using Pong;

return KhaosApplication.CreateBuilder(args)
                       .Build<PongGame>()
                       .Run();
```

## Packages

| Package                                                                                | What it brings                                                                          |
| :------------------------------------------------------------------------------------- | :-------------------------------------------------------------------------------------- |
| [KappaDuck.Khaos](https://www.nuget.org/packages/KappaDuck.Khaos/)                     | The framework: game loop, windowing, input, 2D rendering, assets, hosting, analyzers    |
| [KappaDuck.Khaos.Native](https://www.nuget.org/packages/KappaDuck.Khaos.Native/)       | SDL3 native libraries for Windows, Linux and the browser, referenced by KappaDuck.Khaos |
| [KappaDuck.Khaos.Templates](https://www.nuget.org/packages/KappaDuck.Khaos.Templates/) | `dotnet new khaos`: a game with its desktop and web applications                        |

## Links

- [Documentation](https://khaos.kappaduck.com): API reference, guides and playable examples
- [Roadmap](https://github.com/kappaduck/khaos#roadmap): what each version brings, up to the 2D release ready for Ludum Dare in April 2027
- [Changelog](https://github.com/kappaduck/khaos/blob/main/changelog.md)
- [Source code, samples and SDL versions](https://github.com/kappaduck/khaos)

Khaos is licensed under the MIT license.
