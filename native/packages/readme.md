# KappaDuck.Khaos.Native

Native SDL3 libraries for [KappaDuck.Khaos](https://www.nuget.org/packages/KappaDuck.Khaos/).

[![NuGet Version](https://img.shields.io/nuget/v/KappaDuck.Khaos.Native?style=flat&label=NuGet)](https://www.nuget.org/packages/KappaDuck.Khaos.Native/)

## Overview

This package ships prebuilt native binaries of SDL3 and its extensions ([SDL_image], [SDL_ttf], [SDL_mixer]).
It is referenced by `KappaDuck.Khaos` and does not need to be installed directly.

## Formats

The codecs are compiled into the four libraries:

- **Images**: PNG and APNG, JPEG, WebP, GIF, QOI, BMP, TGA, SVG, ICO, CUR, ANI.
- **Fonts**: TrueType and OpenType with HarfBuzz shaping and colour emoji.
- **Audio**: WAV, AIFF, Ogg Vorbis, Opus, FLAC, MP3, tracker modules (MOD, XM, S3M, IT...).

## Bundled versions

| Library   | Version  |
| :-------- | :------: |
| SDL3      | `3.4.18` |
| SDL_image | `3.4.8`  |
| SDL_ttf   | `3.2.2`  |
| SDL_mixer | `3.2.4`  |

For the versions bundled in other releases and which Khaos release uses them, see the
[SDL compatibility table](https://github.com/kappaduck/khaos#sdl-compatibility).

## Supported platforms

| Runtime identifier |
| :----------------- |
| `win-x64`          |
| `linux-x64`        |

On Linux the libraries need glibc 2.41 or later (Debian 13, Ubuntu 25.04, Fedora 42, Arch). They are
built in the Steam Runtime 4 SDK; see `native/readme.md` in the repository for why and when it changes.

## Versioning

This package follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html) independently of SDL:

- **Patch**: packaging fixes or SDL patch updates with no new native API.
- **Minor**: SDL updates that add native API, or new libraries, formats or platforms.
- **Major**: removed libraries, formats or platforms, or a new SDL major version.

## License

The package is licensed under the MIT license. SDL and its extensions are distributed under the zlib
license. The libraries compiled into them keep their own licenses, all permissive; every one of them is in the
package under `licenses/`.

`dotnet publish` copies those licenses next to your game, under `licenses/sdl/`. Set
`KhaosPublishNativeLicenses` to `false` to handle them yourself. The FreeType License also asks for a
credit in your game's documentation: "Portions of this software are copyright © The FreeType Project
(www.freetype.org)."

[SDL_image]: https://github.com/libsdl-org/SDL_image
[SDL_mixer]: https://github.com/libsdl-org/SDL_mixer
[SDL_ttf]: https://github.com/libsdl-org/SDL_ttf
