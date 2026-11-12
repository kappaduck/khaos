// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.SDL.Marshalling;

namespace KappaDuck.Khaos.Interop.SDL;

internal static partial class SDL3
{
    internal const string SoftwareRenderer = "software";
    internal const string GpuRenderer = "gpu";
    internal const string Direct3D11Renderer = "direct3d11";
    internal const string Direct3D12Renderer = "direct3d12";
    internal const string OpenGLRenderer = "opengl";
    internal const string OpenGLES2Renderer = "opengles2";
    internal const string VulkanRenderer = "vulkan";

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_CreateRenderer", StringMarshalling = StringMarshalling.Utf8)]
    internal static unsafe partial SDL_Renderer* CreateRenderer(SDL_Window* window, string? name);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_DestroyRenderer")]
    internal static unsafe partial void DestroyRenderer(SDL_Renderer* renderer);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetRendererName")]
    [return: MarshalUsing(typeof(BorrowedUtf8StringMarshaller))]
    internal static unsafe partial string? GetRendererName(SDL_Renderer* renderer);

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
