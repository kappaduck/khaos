// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.SDL.Marshalling;
using KappaDuck.Khaos.Mathematics;

namespace KappaDuck.Khaos.Interop.SDL;

internal static partial class SDL3
{
    internal const string WindowCreateFlags = "SDL.window.create.flags";
    internal const string WindowCreateHeight = "SDL.window.create.height";
    internal const string WindowCreateTitle = "SDL.window.create.title";
    internal const string WindowCreateWidth = "SDL.window.create.width";
    internal const string WindowCreateX = "SDL.window.create.x";
    internal const string WindowCreateY = "SDL.window.create.y";
    internal const uint WindowposCenteredMask = 0x2FFF0000u;
    internal const string WindowsVideoDriver = "windows";
    internal const string X11VideoDriver = "x11";
    internal const string WaylandVideoDriver = "wayland";
    internal const string EmscriptenVideoDriver = "emscripten";
    internal const string OffscreenVideoDriver = "offscreen";


    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_CreateWindowWithProperties")]
    internal static safe partial SDL_Window* CreateWindowWithProperties(uint properties);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_DestroyWindow")]
    internal static unsafe partial void DestroyWindow(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetCurrentDisplayMode")]
    internal static safe partial SDL_DisplayMode* GetCurrentDisplayMode(uint display);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetCurrentVideoDriver")]
    [return: MarshalUsing(typeof(BorrowedUtf8StringMarshaller))]
    internal static safe partial string? GetCurrentVideoDriver();

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetDisplayBounds")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static safe partial bool GetDisplayBounds(uint display, out RectI bounds);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetDisplayContentScale")]
    internal static safe partial float GetDisplayContentScale(uint display);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetDisplayForWindow")]
    internal static unsafe partial uint GetDisplayForWindow(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetDisplayName")]
    internal static safe partial byte* GetDisplayName(uint display);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetDisplays")]
    internal static safe partial uint* GetDisplays(out int count);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetDisplayUsableBounds")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static safe partial bool GetDisplayUsableBounds(uint display, out RectI bounds);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetPrimaryDisplay")]
    internal static safe partial uint GetPrimaryDisplay();

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetWindowDisplayScale")]
    internal static unsafe partial float GetWindowDisplayScale(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetWindowFlags")]
    internal static unsafe partial SDL_WindowStates GetWindowFlags(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetWindowMaximumSize")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool GetWindowMaximumSize(SDL_Window* window, out int width, out int height);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetWindowMinimumSize")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool GetWindowMinimumSize(SDL_Window* window, out int width, out int height);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetWindowOpacity")]
    internal static unsafe partial float GetWindowOpacity(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetWindowPixelDensity")]
    internal static unsafe partial float GetWindowPixelDensity(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetWindowPosition")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool GetWindowPosition(SDL_Window* window, out int x, out int y);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetWindowSize")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool GetWindowSize(SDL_Window* window, out int width, out int height);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetWindowSizeInPixels")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool GetWindowSizeInPixels(SDL_Window* window, out int width, out int height);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetWindowTitle")]
    [return: MarshalUsing(typeof(BorrowedUtf8StringMarshaller))]
    internal static unsafe partial string? GetWindowTitle(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_HideWindow")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool HideWindow(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_MaximizeWindow")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool MaximizeWindow(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_MinimizeWindow")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool MinimizeWindow(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_RestoreWindow")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool RestoreWindow(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetWindowBordered")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetWindowBordered(SDL_Window* window, [MarshalAs(UnmanagedType.U1)] bool bordered);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetWindowFocusable")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetWindowFocusable(SDL_Window* window, [MarshalAs(UnmanagedType.U1)] bool focusable);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetWindowFullscreen")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetWindowFullscreen(SDL_Window* window, [MarshalAs(UnmanagedType.U1)] bool fullscreen);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetWindowMaximumSize")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetWindowMaximumSize(SDL_Window* window, int maxWidth, int maxHeight);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetWindowMinimumSize")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetWindowMinimumSize(SDL_Window* window, int minWidth, int minHeight);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetWindowOpacity")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetWindowOpacity(SDL_Window* window, float opacity);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetWindowPosition")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetWindowPosition(SDL_Window* window, int x, int y);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetWindowResizable")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetWindowResizable(SDL_Window* window, [MarshalAs(UnmanagedType.U1)] bool resizable);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetWindowSize")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetWindowSize(SDL_Window* window, int width, int height);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetWindowTitle")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetWindowTitle(SDL_Window* window, in byte title);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_ShowWindow")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool ShowWindow(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SyncWindow")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SyncWindow(SDL_Window* window);
}
