// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL;

internal static partial class SDL3
{
    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetDefaultLogOutputFunction")]
    [UntypedBecause("SDL hands the userdata given to SDL_SetLogOutputFunction back to the callback, whatever it points to")]
    internal static safe partial delegate* unmanaged<void*, SDL_LogCategory, SDL_LogPriority, byte*, void> GetDefaultLogOutputFunction();

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetLogOutputFunction")]
    [UntypedBecause("SDL hands the userdata back to the callback exactly as it was given, whatever it points to")]
    internal static unsafe partial void SetLogOutputFunction(delegate* unmanaged<void*, SDL_LogCategory, SDL_LogPriority, byte*, void> callback, void* userdata);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetLogPriority")]
    internal static safe partial void SetLogPriority(SDL_LogCategory category, SDL_LogPriority priority);
}
