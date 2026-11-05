// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL;

internal static partial class SDL3
{
    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_Init")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static safe partial bool Init(SDL_Subsystems flags);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_InitSubSystem")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static safe partial bool InitSubSystem(SDL_Subsystems flags);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_Quit")]
    internal static safe partial void Quit();

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_QuitSubSystem")]
    internal static safe partial void QuitSubSystem(SDL_Subsystems flags);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetAppMetadataProperty", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static safe partial bool SetAppMetadataProperty(string name, string? value);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_WasInit")]
    internal static safe partial SDL_Subsystems WasInit(SDL_Subsystems flags);
}
