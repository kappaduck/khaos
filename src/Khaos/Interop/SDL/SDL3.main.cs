// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL;

internal static partial class SDL3
{
    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_EnterAppMainCallbacks")]
    [UntypedBecause("SDL hands the app state back to the callbacks exactly as SDL_AppInit stored it, whatever it points to")]
    internal static unsafe partial int EnterAppMainCallbacks(int argc, byte** argv,
        delegate* unmanaged<void**, int, byte**, SDL_AppResult> appInit,
        delegate* unmanaged<void*, SDL_AppResult> appIterate,
        delegate* unmanaged<void*, SDL_Event*, SDL_AppResult> appEvent,
        delegate* unmanaged<void*, SDL_AppResult, void> appQuit);
}
