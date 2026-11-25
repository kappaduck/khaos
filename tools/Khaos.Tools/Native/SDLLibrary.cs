// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Tools.Native;

public sealed record SDLLibrary(string Key, string Repository, string DisplayName)
{
    internal static IReadOnlyList<SDLLibrary> All { get; } =
    [
        new("SDL3", "SDL", "SDL3"),
        new("SDL3_IMAGE", "SDL_image", "SDL_image"),
        new("SDL3_TTF", "SDL_ttf", "SDL_ttf"),
        new("SDL3_MIXER", "SDL_mixer", "SDL_mixer"),
    ];
}
