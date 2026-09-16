<p align="center">
  <img src="https://raw.githubusercontent.com/kappaduck/khaos/main/assets/logo/khaos-logo-256.png" alt="Khaos logo" width="128" height="128">
</p>

<h1 align="center">Khaos</h1>

<p align="center">A modern, fast and flexible .NET game framework using SDL3</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-11.0-512BD4" alt=".NET 11.0">
  <a href="https://www.nuget.org/packages/KappaDuck.Khaos/"><img src="https://img.shields.io/nuget/v/KappaDuck.Khaos?style=flat&label=NuGet" alt="NuGet Version"></a>
</p>

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

Install Khaos via [NuGet]:

```bash
dotnet package add KappaDuck.Khaos -v 0.1.0
```

or via your `.csproj`:

```xml
<PackageReference Include="KappaDuck.Khaos" Version="0.1.0" />
```

You can also install via the NuGet Package Manager in Visual Studio or JetBrains Rider.

> [!WARNING]
> Khaos is still in early development. Expect breaking changes and frequent updates. Always use the latest version for the best experience.

### Beta packages

Pre-release versions are published to NuGet.org alongside stable releases. To install the latest beta:

```bash
dotnet package add KappaDuck.Khaos --prerelease
```

## Documentation

Full API documentation and samples are available:

- [Full API reference][khaos.kappaduck.com]
- **[`samples/`][samples]** for runnable code covering common use cases
- **[`demo/`][demo]** for complex examples

## Cross-platform support

Khaos supports Windows and Linux thanks to SDL3's abstraction layer.

The framework may have platform-specific implementations or limitations depending on the underlying SDL support. Using platform-specific features will surface a compiler warning indicating the code may not be portable across all targets.

## SDL compatibility

SDL3 native libraries are bundled via `KappaDuck.Khaos.Runtimes`. The table below shows the SDL versions included in each release of both packages.

|  Khaos   | Runtimes |   SDL3   | SDL_image | SDL_ttf | SDL_mixer |
| :------: | :------: | :------: | :-------: | :-----: | :-------: |
| `source` | `1.0.0`  | `3.4.16` |  `3.4.6`  | `3.2.2` |  `3.2.4`  |

## Development & Sandbox

You can build Khaos from source and experiment quickly using the included sandbox project.

### Prerequisites

- [.NET 11.0 SDK](https://dotnet.microsoft.com/download/dotnet/11.0)

### Setup

**1. Clone the repository**

```bash
git clone https://github.com/KappaDuck/khaos.git
cd khaos
```

**2. Build and test**

```bash
dotnet build
dotnet test
```

Tests run headless by default, so they work on a machine without a display. To watch the windows appear, pick a driver:

```bash
dotnet test -- --test-parameter video-driver=default
```

`default` lets the platform choose, the way a real application does. You can also name a driver directly, such as `x11` or `windows`. An `SDL_VIDEODRIVER` environment variable that is already set always wins.

**3. Restore the local tools** (only needed to build the documentation)

```bash
dotnet tool restore
dotnet build src/Khaos --configuration Release
dotnet docfx docs/docfx.json --serve
```

The API reference is generated from the compiled assembly and its XML documentation, so the build has to run first.

### Khaos.Sandbox

The repository includes a dedicated sandbox project at `src/Khaos.Sandbox/` for experimenting without touching the main source. It references `KappaDuck.Khaos` directly so changes are reflected immediately.

Everything you write there is ignored by git. If the folder holds no source at all, the first build drops a starter `Program.cs` in for you, so a fresh clone always compiles.

```bash
dotnet run --project src/Khaos.Sandbox
```

or simply run it in your IDE.

### Benchmarks

```bash
dotnet run -c Release --project benchmarks -- --filter '*'
```

## AI disclosure

AI tools assisted with two things in this project: **documentation** (XML doc comments, README, CONTRIBUTING guidelines) and **design exploration** (prototyping API shapes, exploring implementation approaches, and thinking through architecture decisions). Everything is reviewed by the author and no generated-AI code will be in the code source.

## Credits

Built with inspiration from

- [SDL3]
- [SDL_image]
- [SDL_ttf]
- [SDL_mixer]
- [SFML](https://www.sfml-dev.org/)
- [LazyFoo](https://lazyfoo.net/index.php)
- [Sayers.SDL2.Core](https://github.com/JeremySayers/Sayers.SDL2.Core)
- [SDL3-CS](https://github.com/flibitijibibo/SDL3-CS)

[samples]: samples
[demo]: demo
[NuGet]: https://www.nuget.org/packages/KappaDuck.Khaos/
[SDL3]: https://www.libsdl.org/
[SDL_image]: https://github.com/libsdl-org/SDL_image
[SDL_mixer]: https://github.com/libsdl-org/SDL_mixer
[SDL_ttf]: https://github.com/libsdl-org/SDL_ttf
[khaos.kappaduck.com]: https://khaos.kappaduck.com
