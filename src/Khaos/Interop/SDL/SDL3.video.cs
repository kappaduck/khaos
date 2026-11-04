// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;

namespace KappaDuck.Khaos.Interop.SDL;

internal static partial class SDL3
{
    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetCurrentDisplayMode")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static safe partial SDL_DisplayMode* GetCurrentDisplayMode(uint display);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetDisplayBounds")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static safe partial bool GetDisplayBounds(uint display, out RectI bounds);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetDisplayContentScale")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static safe partial float GetDisplayContentScale(uint display);

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
}
