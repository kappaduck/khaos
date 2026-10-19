#!/usr/bin/env python3
"""Loads the SDL libraries of KappaDuck.Khaos.Native and checks every format is compiled in.

    python3 native/scripts/smoke-test.py <folder with the libraries>

Run by native.yml on Windows and in containers of several Linux distributions. It fails when:
- a library does not load: a glibc that is too old, a DLL or .so the system does not have, or a
  satellite that cannot find SDL3 next to it (the satellites are loaded first on purpose);
- an image format does not round-trip, or a loader without a saver is missing;
- SDL_ttf was built without HarfBuzz;
- an SDL_mixer decoder is missing.

The list of formats is native/readme.md's: change both together.
"""

import ctypes
import os
import sys
import tempfile

SDL_INIT_AUDIO = 0x10
SDL_INIT_VIDEO = 0x20
SDL_PIXELFORMAT_RGBA32 = 0x16762004  # ABGR8888 on little-endian

SAVED_IMAGES = ["png", "jpg", "webp", "gif", "tga", "bmp", "ico", "cur"]

DETECTED_IMAGES = {
    "QOI": b"qoif" + bytes(12),
    "SVG": b'<svg xmlns="http://www.w3.org/2000/svg"></svg>',
}

DECODERS = {"WAV", "AIFF", "DRFLAC", "DRMP3", "VORBIS", "OPUS", "XMP", "RAW", "SINEWAVE"}

def library(folder: str, name: str) -> ctypes.CDLL:
    file = f"{name}.dll" if os.name == "nt" else f"lib{name}.so"
    return ctypes.CDLL(os.path.join(folder, file))

def main() -> int:
    folder = os.path.abspath(sys.argv[1])
    errors = []

    os.environ["SDL_VIDEODRIVER"] = "dummy"
    os.environ["SDL_AUDIODRIVER"] = "dummy"

    image = library(folder, "SDL3_image")
    ttf = library(folder, "SDL3_ttf")
    mixer = library(folder, "SDL3_mixer")
    sdl = library(folder, "SDL3")

    sdl.SDL_GetError.restype = ctypes.c_char_p
    sdl.SDL_Init.restype = ctypes.c_bool
    sdl.SDL_CreateSurface.restype = ctypes.c_void_p
    sdl.SDL_CreateSurface.argtypes = [ctypes.c_int, ctypes.c_int, ctypes.c_uint32]
    sdl.SDL_FillSurfaceRect.argtypes = [ctypes.c_void_p, ctypes.c_void_p, ctypes.c_uint32]
    sdl.SDL_DestroySurface.argtypes = [ctypes.c_void_p]
    sdl.SDL_IOFromConstMem.restype = ctypes.c_void_p
    sdl.SDL_IOFromConstMem.argtypes = [ctypes.c_char_p, ctypes.c_size_t]
    sdl.SDL_CloseIO.argtypes = [ctypes.c_void_p]
    image.IMG_Save.restype = ctypes.c_bool
    image.IMG_Save.argtypes = [ctypes.c_void_p, ctypes.c_char_p]
    image.IMG_Load.restype = ctypes.c_void_p
    image.IMG_Load.argtypes = [ctypes.c_char_p]
    ttf.TTF_Init.restype = ctypes.c_bool
    mixer.MIX_Init.restype = ctypes.c_bool
    mixer.MIX_GetAudioDecoder.restype = ctypes.c_char_p

    def error() -> str:
        return (sdl.SDL_GetError() or b"").decode()

    def version(number: int) -> str:
        return f"{number // 1000000}.{number // 1000 % 1000}.{number % 1000}"

    if not sdl.SDL_Init(SDL_INIT_VIDEO | SDL_INIT_AUDIO):
        print(f"SDL_Init failed: {error()}", file=sys.stderr)
        return 1

    print(f"SDL {version(sdl.SDL_GetVersion())}, SDL_image {version(image.IMG_Version())}, "
          f"SDL_ttf {version(ttf.TTF_Version())}, SDL_mixer {version(mixer.MIX_Version())}")

    surface = sdl.SDL_CreateSurface(16, 16, SDL_PIXELFORMAT_RGBA32)
    sdl.SDL_FillSurfaceRect(surface, None, 0xFF3366CC)
    with tempfile.TemporaryDirectory() as temp:
        for extension in SAVED_IMAGES:
            path = os.path.join(temp, f"probe.{extension}").encode()
            if not image.IMG_Save(surface, path):
                errors.append(f"SDL_image cannot save .{extension}: {error()}")
                continue
            loaded = image.IMG_Load(path)
            if not loaded:
                errors.append(f"SDL_image cannot load .{extension}: {error()}")
                continue
            sdl.SDL_DestroySurface(loaded)
    sdl.SDL_DestroySurface(surface)

    for name, header in DETECTED_IMAGES.items():
        detect = getattr(image, f"IMG_is{name}")
        detect.restype = ctypes.c_bool
        detect.argtypes = [ctypes.c_void_p]
        stream = sdl.SDL_IOFromConstMem(header, len(header))
        if not detect(stream):
            errors.append(f"SDL_image was built without {name}")
        sdl.SDL_CloseIO(stream)

    if not ttf.TTF_Init():
        errors.append(f"TTF_Init failed: {error()}")
    major, minor, patch = ctypes.c_int(), ctypes.c_int(), ctypes.c_int()
    ttf.TTF_GetHarfBuzzVersion(ctypes.byref(major), ctypes.byref(minor), ctypes.byref(patch))
    if major.value == 0:
        errors.append("SDL_ttf was built without HarfBuzz")
    else:
        print(f"HarfBuzz {major.value}.{minor.value}.{patch.value}")

    if not mixer.MIX_Init():
        errors.append(f"MIX_Init failed: {error()}")
    decoders = {mixer.MIX_GetAudioDecoder(i).decode() for i in range(mixer.MIX_GetNumAudioDecoders())}
    print(f"SDL_mixer decoders: {', '.join(sorted(decoders))}")
    for missing in sorted(DECODERS - decoders):
        errors.append(f"SDL_mixer was built without the {missing} decoder")

    mixer.MIX_Quit()
    ttf.TTF_Quit()
    sdl.SDL_Quit()

    for message in errors:
        print(f"error: {message}", file=sys.stderr)
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main())
