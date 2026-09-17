# Khaos

A modern, fast and flexible .NET game framework using SDL3

![.NET 11.0](https://img.shields.io/badge/.NET-11.0-512BD4)
[![NuGet Version](https://img.shields.io/nuget/v/KappaDuck.Khaos?style=flat&label=NuGet)](https://www.nuget.org/packages/KappaDuck.Khaos/)

## Overview

Khaos is a modern, simple and fast game framework for building game and interactive
applications, built on top of SDL3 and its extensions ([SDL_image], [SDL_mixer], [SDL_ttf]).
It targets .NET 11+ desktop, providing a clean and flexible API that hides the complexity of SDL.

## Usage

```csharp
using KappaDuck.Khaos;
using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Graphics.Rendering;

[AssetCatalog("assets")]
internal static partial class Content;

internal sealed class Pong : Game2D
{
    private Sprite _ball = null!;

    public Pong() : base("Pong!", 1920, 1080)
    {
    }

    protected override void Load(LoadContext context)
    {
        _ball = Assets.Load(Content.Sprites.Ball);
    }

    protected override void OnEvent(in Event e)
    {
        if (e is KeyPressed { Key: Key.Escape })
        {
            Exit();
            return;
        }
    }

    protected override void FixedUpdate(in GameTime time)
    {
        // move the ball!
    }

    protected override void Draw(Renderer renderer, in Frame frame)
    {
        renderer.Clear();

        renderer.Draw(_ball);
        renderer.Present();
    }
}

// Program.cs
using Pong game = new();
game.Run();
```

More samples and documentation can be found at [Documentation](#documentation)

> [!NOTE]
> The public API is still a prototype. Expect to have frequent changes.

## Installation

```bash
dotnet package add KappaDuck.Khaos
```

> **Warning:** Khaos is still in early development. Expect breaking changes and frequent updates.

Pre-release versions are published alongside stable releases:

```bash
dotnet package add KappaDuck.Khaos --prerelease
```

## Documentation

- [Full API reference](https://khaos.kappaduck.com)
- [Samples](https://github.com/kappaduck/khaos/tree/main/samples) for runnable code covering common use cases
- [Demo](https://github.com/kappaduck/khaos/tree/main/demo) for complex examples
- [Source code and contributing](https://github.com/kappaduck/khaos)

## Cross-platform support

Khaos supports Windows and Linux thanks to SDL3's abstraction layer. Using platform-specific
features surfaces a compiler warning indicating the code may not be portable across all targets.

## SDL compatibility

SDL3 native libraries are bundled via [KappaDuck.Khaos.Runtimes](https://www.nuget.org/packages/KappaDuck.Khaos.Runtimes/).
See the [SDL compatibility table](https://github.com/kappaduck/khaos#sdl-compatibility) for the SDL versions used by each release.

## License

Khaos is licensed under the MIT license.

[SDL_image]: https://github.com/libsdl-org/SDL_image
[SDL_mixer]: https://github.com/libsdl-org/SDL_mixer
[SDL_ttf]: https://github.com/libsdl-org/SDL_ttf
