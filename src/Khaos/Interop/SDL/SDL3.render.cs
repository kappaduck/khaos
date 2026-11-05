// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL;

internal static partial class SDL3
{
    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_CreateRenderer", StringMarshalling = StringMarshalling.Utf8)]
    internal static unsafe partial SDL_Renderer* CreateRenderer(SDL_Window* window, string? name);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_DestroyRenderer")]
    internal static unsafe partial void DestroyRenderer(SDL_Renderer* renderer);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_RenderClear")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool RenderClear(SDL_Renderer* renderer);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_RenderPresent")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool RenderPresent(SDL_Renderer* renderer);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetRenderDrawColor")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetRenderDrawColor(SDL_Renderer* renderer, byte r, byte g, byte b, byte a);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetRenderVSync")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetRenderVSync(SDL_Renderer* renderer, int vsync);
}
