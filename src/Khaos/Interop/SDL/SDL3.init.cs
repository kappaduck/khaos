// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.SDL.Primitives;

namespace KappaDuck.Khaos.Interop.SDL;

internal static partial class SDL3
{
    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_Init")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static safe partial bool Init(SDL_Subsystems flags);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_InitSubSystem")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static safe partial bool InitSubSystem(SDL_Subsystems flags);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_Quit")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static safe partial void Quit();

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_QuitSubSystem")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static safe partial void QuitSubSystem(SDL_Subsystems flags);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetAppMetadataProperty", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static safe partial bool SetAppMetadataProperty(string name, string? value);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_WasInit")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static safe partial SDL_Subsystems WasInit(SDL_Subsystems flags);
}
