# Native libraries

SDL3, SDL_image, SDL_ttf and SDL_mixer, built from the versions in `sdl.env` (and, for WebAssembly, with the Emscripten
versions in `emscripten.env`):

| Built by                   | For                                     | In the package        |
| :------------------------- | :-------------------------------------- | :-------------------- |
| `scripts/build-windows.sh` | win-x64, cross-compiled with MinGW      | `runtimes/win-x64/`   |
| `scripts/build-linux.sh`   | linux-x64, in the Steam Runtime SDK     | `runtimes/linux-x64/` |
| `scripts/build-wasm.sh`    | WebAssembly, one build per .NET version | `wasm/<tfm>/`         |

`auto-update.cs` checks SDL's releases every week and opens a pull request that updates `sdl.env`,
`packages/Native.nuspec` and the version tables.

## What the desktop libraries contain

Four files per platform, and nothing else: `SDL3`, `SDL3_image`, `SDL3_ttf` and `SDL3_mixer`. The formats
below are compiled into them, so a game loads any of them on any player's machine without shipping or
installing anything more. Every other format SDL can read is turned off.

| Library   | Formats                                                                                             | Through                                               |
| :-------- | :-------------------------------------------------------------------------------------------------- | :---------------------------------------------------- |
| SDL_image | PNG and APNG, JPEG, WebP and animated WebP, GIF and animated GIF, QOI, BMP, TGA, SVG, ICO, CUR, ANI | libpng, stb_image, libwebp, nanosvg, built-in         |
| SDL_ttf   | TrueType and OpenType, complex scripts (shaping), colour emoji                                      | FreeType, HarfBuzz, plutosvg                          |
| SDL_mixer | WAV, AIFF, Ogg Vorbis, Opus, FLAC, MP3, MOD/XM/S3M/IT and other tracker modules                     | built-in, libvorbis, libopus, dr_flac, dr_mp3, libxmp |

SDL_image also saves PNG, JPEG, WebP, GIF, TGA, BMP, ICO, CUR and ANI.

### How

The build scripts turn off what SDL's CMake does by default:

- `*_VENDORED=ON` alone is not enough. With `SDLIMAGE_DEPS_SHARED` and `SDLMIXER_DEPS_SHARED` at their
  default (on), the codecs are built as separate libraries (`libwebp-7.dll`, `libpng16.so.16`...) that SDL
  opens by name the first time a file of that format is loaded. Windows looks for them next to the
  executable, Linux only in the system folders, and `dotnet run` leaves native libraries in
  `runtimes/<rid>/native/` where neither looks: the format then fails at load time, or on Linux silently
  picks up whatever version the distribution has. The scripts set `*_DEPS_SHARED=OFF`.
- `*_STRICT=ON` makes a missing dependency fail the build instead of silently leaving a format out.
- Only the vendored libraries in use are cloned (`git submodule update` on each), at the commits SDL pins;
  `external/download.sh` would clone all of them.
- The compiler runtime is linked in. HarfBuzz is C++, and some libraries use threads or the stack
  protector: on Windows, without `-static` in `mingw.cmake`, the DLLs can import `libstdc++-6.dll`,
  `libwinpthread-1.dll` or `libssp-0.dll`, which exist only where MinGW is installed (the first build
  did). On Linux, `-static-libstdc++` removes the dependency on the player's libstdc++ version.
- Linux: one file per library, named the way .NET looks for it (`[LibraryImport("SDL3")]` loads
  `libSDL3.so`). SDL installs `libSDL3.so` → `libSDL3.so.0` → `libSDL3.so.0.4.18`, and neither an
  artifact upload nor a NuGet package keeps links: each name becomes a full copy. The script copies the
  real file once, renames its SONAME with `patchelf`, and points the satellites at `libSDL3.so` with
  `RUNPATH=$ORIGIN`, so they load in any order, from the game folder or from `runtimes/linux-x64/native/`.

### Licenses

The libraries compiled in are all under permissive licenses (zlib, libpng, BSD, MIT, the FreeType
License). Most still ask for their notice to travel with the binaries.

The package holds every license under `licenses/` (`SDL3_image/optional/LICENSE.webp.txt`...), and
`dotnet publish` copies them next to the game under `licenses/sdl/` (`packages/KappaDuck.Khaos.Native.targets`;
`KhaosPublishNativeLicenses=false` turns it off). The FreeType License also asks for a credit in the game's
documentation: "Portions of this software are copyright © The FreeType Project (www.freetype.org)."

### WebAssembly is lighter

`build-wasm.sh` keeps a subset of the desktop formats: everything is linked into `dotnet.native.wasm`,
which every player downloads, so the formats that need an outside library stay out. PNG and JPEG go through
stb_image (no APNG, no WebP); audio is WAV, AIFF, Ogg Vorbis (stb_vorbis), FLAC and MP3 (no Opus, no
tracker modules); fonts have no HarfBuzz and no colour emoji.

## Linux: the glibc baseline

A Linux library runs only where the system's glibc is at least the one it was built against (libstdc++ is
linked in, so only glibc matters). The Linux libraries are therefore built in a container whose glibc is
the oldest one a player may have, and that container decides which distributions run a Khaos game.

**Today: Steam Runtime 4** (`registry.gitlab.steamos.cloud/steamrt/steamrt4/sdk`, Debian 13, glibc 2.41)

| Distribution                      |   glibc    | Runs the libraries |
| :-------------------------------- | :--------: | :----------------: |
| Debian 13                         |    2.41    |        yes         |
| Ubuntu 25.04, 25.10               | 2.41, 2.42 |        yes         |
| Fedora 42 and later               |   2.41+    |        yes         |
| Arch Linux, rolling distributions |   latest   |        yes         |
| Ubuntu 24.04 LTS                  |    2.39    |         no         |
| Debian 12                         |    2.36    |         no         |
| Fedora 41                         |    2.40    |         no         |

On a system below the baseline, .NET reports a `DllNotFoundException` for SDL, and `ldd` on the library
shows `version 'GLIBC_2.41' not found`.

## When to move the glibc baseline

**Down**, to Steam Runtime 3 (`steamrt/sniper/sdk`, Debian 11, glibc 2.31), when the games must run on
LTS systems outside Steam: players report the `GLIBC_2.41` error above, or a jam's audience is on
Ubuntu 24.04.

**Up**, when Valve publishes the SDK of the next Steam Runtime and SDL's own CI moves to it, or when an
SDL update needs system headers the current image lacks. Look at it on each SDL update pull request and
at each .NET major version.

**How**, in one pull request:

1. the image in `native.yml` (`build-linux` and the `smoke-test-linux` matrix) and `GLIBC_MAX`;
2. the distributions of the smoke test: the oldest one at or above the new baseline, and newer ones;
3. the table above;
4. a major version of `KappaDuck.Khaos.Native` when the baseline goes up: dropping distributions is
   dropping platforms, which the package's versioning treats as breaking (`packages/readme.md`).

On the first build in a new image, read the SDL configure summary in the log: a missing development
package does not fail the build, it silently leaves out a backend (Wayland, PipeWire...).
`dependencies.sh linux` installs what the image lacks.

## Games published with NativeAOT

A game's own executable follows the same rule: NativeAOT links it against the glibc of the machine that
publishes it. Published on Ubuntu 24.04, it needs glibc 2.39 whatever the libraries need. See
`docs/guides/linux.md`.
