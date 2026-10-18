# Khaos

A modern, fast and flexible .NET game framework using SDL3.

Khaos is a .NET 11 game framework built on [SDL3](https://www.libsdl.org/) and its extensions
([SDL_image](https://github.com/libsdl-org/SDL_image), [SDL_mixer](https://github.com/libsdl-org/SDL_mixer),
[SDL_ttf](https://github.com/libsdl-org/SDL_ttf)). Your game is plain C#: no pointers, no native handles, no C-style
calls. The framework takes care of the window, the loop and the platform.

- **2D first, then 3D**: `Game2D` builds on [SDL_Renderer](https://wiki.libsdl.org/SDL3/CategoryRender), `Game3D` will
  build on [SDL_GPU](https://wiki.libsdl.org/SDL3/CategoryGPU) once 2D is done.
- **Low allocation**: the framework avoids allocating on every frame, so the garbage collector stays out of the way.
- **Generated events**: `OnEvent` pattern-matches on cases like `KeyPressed` without boxing or allocating.
- **Typed assets**: `[AssetCatalog]` turns the assets folder into properties, so a missing file breaks the build, not the game.
- **One game, every platform**: `dotnet new khaos` sets up the game with a desktop and a web application.
- **Hosting built in**: dependency injection, logging and configuration from `Microsoft.Extensions`.

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
[AssetCatalog("assets")]
internal static partial class Content;

internal sealed class PongGame() : Game2D("Pong!", 1280, 720)
{
    private Sprite _ball = null!;
    private Vector2 _previous = new(640f, 360f);
    private Vector2 _position = new(640f, 360f);
    private Vector2 _velocity = new(240f, 180f);

    protected override void Load() => _ball = new Sprite(Assets.Load(Content.Sprites.Ball));

    protected override void OnEvent(in Event e)
    {
        if (e is KeyPressed { Scancode: Scancode.Escape })
            Exit();
    }

    protected override void FixedUpdate(in GameTime time)
    {
        _previous = _position;
        _position += _velocity * (float)time.Delta;
    }

    protected override void Draw(Renderer renderer, in Frame frame)
    {
        _ball.Position = Vector2.Lerp(_previous, _position, frame.Alpha);

        renderer.Clear(Color.Slate);
        renderer.Draw(_ball);
        renderer.Present();
    }
}
```

```csharp
// Program.cs of the desktop or web application
KhaosApplicationBuilder builder = KhaosApplication.CreateBuilder(args);

return builder.Build<PongGame>().Run();
```

## Packages

| Package                                                                          | What it brings                                                                                    |
| :------------------------------------------------------------------------------- | :------------------------------------------------------------------------------------------------ |
| [KappaDuck.Khaos](https://www.nuget.org/packages/KappaDuck.Khaos/)               | The framework: game loop, windowing, input, 2D rendering, audio, text, assets, hosting, analyzers |
| [KappaDuck.Khaos.Native](https://www.nuget.org/packages/KappaDuck.Khaos.Native/) | SDL3 native libraries for Windows and Linux, referenced by KappaDuck.Khaos                        |

## Links

- [Documentation](https://khaos.kappaduck.com): API reference, guides and playable examples
- [Roadmap](https://github.com/kappaduck/khaos#roadmap): what each version brings, up to the 2D release of March 2027
- [Changelog](https://github.com/kappaduck/khaos/blob/main/changelog.md)
- [Source code, samples and SDL versions](https://github.com/kappaduck/khaos)

Khaos is licensed under the MIT license.
