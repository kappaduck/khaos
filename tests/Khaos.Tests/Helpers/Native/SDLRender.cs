// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Graphics.Rendering;
using KappaDuck.Khaos.Interop.SDL;
using KappaDuck.Khaos.Interop.SDL.Primitives;
using KappaDuck.Khaos.Mathematics;
using KappaDuck.Khaos.Windowing;
using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Tests.Helpers.Native;

internal static partial class SDLRender
{
    internal static Color ReadPixel(Renderer renderer, int x, int y)
    {
        unsafe
        {
            SDL_Surface* surface = RenderReadPixels(renderer.Handle, null);

            if (surface is null)
                throw new InvalidOperationException("SDL_RenderReadPixels could not read the render target.");

            try
            {
                if (!ReadSurfacePixel(surface, x, y, out byte r, out byte g, out byte b, out byte a))
                    throw new InvalidOperationException($"SDL_ReadSurfacePixel could not read the pixel at ({x}, {y}).");

                return new Color(r, g, b, a);
            }
            finally
            {
                DestroySurface(surface);
            }
        }
    }

    internal static string? NameOf(Window window)
    {
        unsafe
        {
            return SDL3.GetRendererName(SDL3.ThrowIfNull(GetRenderer(window.Handle)));
        }
    }

    internal static int ReadVSync(Renderer renderer)
    {
        unsafe
        {
            if (!GetRenderVSync(renderer.Handle, out int vsync))
                throw new InvalidOperationException("SDL_GetRenderVSync could not read the VSync of the renderer.");

            return vsync;
        }
    }

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_DestroySurface")]
    private static unsafe partial void DestroySurface(SDL_Surface* surface);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetRenderer")]
    private static unsafe partial SDL_Renderer* GetRenderer(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetRenderVSync")]
    [return: MarshalAs(UnmanagedType.U1)]
    private static unsafe partial bool GetRenderVSync(SDL_Renderer* renderer, out int vsync);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_ReadSurfacePixel")]
    [return: MarshalAs(UnmanagedType.U1)]
    private static unsafe partial bool ReadSurfacePixel(SDL_Surface* surface, int x, int y, out byte r, out byte g, out byte b, out byte a);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_RenderReadPixels")]
    private static unsafe partial SDL_Surface* RenderReadPixels(SDL_Renderer* renderer, RectI* rect);

    private readonly struct SDL_Surface;
}
