# KappaDuck.Khaos.Runtimes

Native SDL3 libraries for [KappaDuck.Khaos](https://www.nuget.org/packages/KappaDuck.Khaos/).

[![NuGet Version](https://img.shields.io/nuget/v/KappaDuck.Khaos.Runtimes?style=flat&label=NuGet)](https://www.nuget.org/packages/KappaDuck.Khaos.Runtimes/)

## Overview

This package ships prebuilt native binaries of SDL3 and its extensions ([SDL_image], [SDL_ttf], [SDL_mixer]).
It is referenced by `KappaDuck.Khaos` and does not need to be installed directly.

## Bundled versions

| Library   | Version  |
| :-------- | :------: |
| SDL3      | `3.4.16` |
| SDL_image | `3.4.6`  |
| SDL_ttf   | `3.2.2`  |
| SDL_mixer | `3.2.4`  |

For the versions bundled in other releases and which Khaos release uses them, see the
[SDL compatibility table](https://github.com/kappaduck/khaos#sdl-compatibility).

## Supported platforms

| Runtime identifier |
| :----------------- |
| `win-x64`          |
| `linux-x64`        |

## Versioning

This package follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html) independently of SDL:

- **Patch**: packaging fixes or SDL patch updates with no new native API.
- **Minor**: SDL updates that add native API, or new libraries or platforms.
- **Major**: removed libraries or platforms, or a new SDL major version.

## License

The package is licensed under the MIT license. The bundled SDL libraries are distributed under the zlib license.

[SDL_image]: https://github.com/libsdl-org/SDL_image
[SDL_mixer]: https://github.com/libsdl-org/SDL_mixer
[SDL_ttf]: https://github.com/libsdl-org/SDL_ttf
