# KappaDuck.Khaos.Native

SDL3 for [KappaDuck.Khaos](https://www.nuget.org/packages/KappaDuck.Khaos/), on the desktop and in the browser.

[![NuGet Version](https://img.shields.io/nuget/v/KappaDuck.Khaos.Native?style=flat&label=NuGet)](https://www.nuget.org/packages/KappaDuck.Khaos.Native/)

## Overview

This package ships SDL3 and its extensions ([SDL_image], [SDL_ttf], [SDL_mixer]), prebuilt for every platform Khaos
runs on. It is referenced by [KappaDuck.Khaos](https://www.nuget.org/packages/KappaDuck.Khaos/) and does not need to
be installed directly.

A game only gets what it runs on:

- **Desktop**: native libraries under `runtimes/<rid>/native/`. `dotnet publish` copies the ones of the runtime identifier
  it publishes for.
- **WebAssembly**: static libraries under `wasm/<tfm>/`, which the `wasm-tools` workload links into the game's
  `dotnet.native.wasm`. They are never copied: players download what is linked in, nothing more.

## Formats

On the desktop, the codecs are compiled into the four libraries:

- **Images**: PNG and APNG, JPEG, WebP, GIF, QOI, BMP, TGA, SVG, ICO, CUR, ANI.
- **Fonts**: TrueType and OpenType with HarfBuzz shaping and colour emoji.
- **Audio**: WAV, AIFF, Ogg Vorbis, Opus, FLAC, MP3, tracker modules (MOD, XM, S3M, IT...).

Every player downloads what is linked in, so the WebAssembly build keeps the formats that need no outside library:

- **Images**: PNG, JPEG, GIF, QOI, BMP, TGA, SVG, ICO, CUR, ANI.
- **Fonts**: TrueType and OpenType through FreeType, without HarfBuzz shaping or colour emoji.
- **Audio**: WAV, AIFF, Ogg Vorbis, FLAC, MP3.

## Bundled versions

| Library   | Version  |
| :-------- | :------: |
| SDL3      | `3.4.18` |
| SDL_image | `3.4.8`  |
| SDL_ttf   | `3.2.2`  |
| SDL_mixer | `3.2.4`  |

The same version on every platform. For the versions bundled in other releases and which Khaos release uses them,
see the [SDL compatibility table](https://github.com/kappaduck/khaos#sdl-compatibility).

## Supported platforms

| Platform    | Runtime identifier or target framework | Notes                            |
| :---------- | :------------------------------------- | :------------------------------- |
| Windows     | `win-x64`                              |                                  |
| Linux       | `linux-x64`                            | glibc 2.41 or later              |
| WebAssembly | `net11.0`                              | compiled with Emscripten `6.0.3` |

On Linux the libraries need glibc 2.41 or later (Debian 13, Ubuntu 25.04, Fedora 42, Arch). They are built in the
Steam Runtime 4 SDK; see `native/readme.md` in the repository for why and when it changes.

For WebAssembly, the `wasm-tools` workload picks its Emscripten from the game's target framework, so the libraries are
compiled once per supported .NET version, with that version's Emscripten.

## Versioning

This package follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html) independently of SDL:

- **Patch**: packaging fixes or SDL patch updates with no new native API.
- **Minor**: SDL updates that add native API, or new libraries, formats, platforms or .NET versions.
- **Major**: removed libraries, formats, platforms or .NET versions, or a new SDL major version.

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
