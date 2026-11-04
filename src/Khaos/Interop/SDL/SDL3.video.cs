// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.SDL.Marshalling;
using KappaDuck.Khaos.Mathematics;

namespace KappaDuck.Khaos.Interop.SDL;

internal static partial class SDL3
{
    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_CreateWindowWithProperties")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static safe partial SDL_Window* CreateWindowWithProperties(uint properties);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_DestroyWindow")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial void DestroyWindow(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetCurrentDisplayMode")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static safe partial SDL_DisplayMode* GetCurrentDisplayMode(uint display);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetCurrentVideoDriver")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(BorrowedUtf8StringMarshaller))]
    internal static safe partial string? GetCurrentVideoDriver();

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetDisplayBounds")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static safe partial bool GetDisplayBounds(uint display, out RectI bounds);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetDisplayContentScale")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static safe partial float GetDisplayContentScale(uint display);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetDisplayForWindow")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial uint GetDisplayForWindow(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetDisplayName")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static safe partial byte* GetDisplayName(uint display);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetDisplays")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static safe partial uint* GetDisplays(out int count);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetDisplayUsableBounds")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static safe partial bool GetDisplayUsableBounds(uint display, out RectI bounds);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetPrimaryDisplay")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static safe partial uint GetPrimaryDisplay();

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetWindowDisplayScale")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial float GetWindowDisplayScale(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetWindowFlags")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial SDL_WindowStates GetWindowFlags(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetWindowMaximumSize")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool GetWindowMaximumSize(SDL_Window* window, out int width, out int height);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetWindowMinimumSize")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool GetWindowMinimumSize(SDL_Window* window, out int width, out int height);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetWindowOpacity")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial float GetWindowOpacity(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetWindowPixelDensity")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial float GetWindowPixelDensity(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetWindowPosition")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool GetWindowPosition(SDL_Window* window, out int x, out int y);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetWindowSize")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool GetWindowSize(SDL_Window* window, out int width, out int height);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetWindowSizeInPixels")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool GetWindowSizeInPixels(SDL_Window* window, out int width, out int height);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetWindowTitle")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(BorrowedUtf8StringMarshaller))]
    internal static unsafe partial string? GetWindowTitle(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_HideWindow")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool HideWindow(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_MaximizeWindow")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool MaximizeWindow(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_MinimizeWindow")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool MinimizeWindow(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_RestoreWindow")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool RestoreWindow(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetWindowBordered")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetWindowBordered(SDL_Window* window, [MarshalAs(UnmanagedType.U1)] bool bordered);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetWindowFocusable")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetWindowFocusable(SDL_Window* window, [MarshalAs(UnmanagedType.U1)] bool focusable);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetWindowFullscreen")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetWindowFullscreen(SDL_Window* window, [MarshalAs(UnmanagedType.U1)] bool fullscreen);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetWindowMaximumSize")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetWindowMaximumSize(SDL_Window* window, int maxWidth, int maxHeight);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetWindowMinimumSize")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetWindowMinimumSize(SDL_Window* window, int minWidth, int minHeight);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetWindowOpacity")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetWindowOpacity(SDL_Window* window, float opacity);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetWindowPosition")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetWindowPosition(SDL_Window* window, int x, int y);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetWindowResizable")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetWindowResizable(SDL_Window* window, [MarshalAs(UnmanagedType.U1)] bool resizable);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetWindowSize")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetWindowSize(SDL_Window* window, int width, int height);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetWindowTitle")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SetWindowTitle(SDL_Window* window, in byte title);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_ShowWindow")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool ShowWindow(SDL_Window* window);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SyncWindow")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static unsafe partial bool SyncWindow(SDL_Window* window);
}
