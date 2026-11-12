// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL;

internal static partial class SDL3
{
    internal const string AppMetadataCopyright = "SDL.app.metadata.copyright";
    internal const string AppMetadataCreator = "SDL.app.metadata.creator";
    internal const string AppMetadataIdentifier = "SDL.app.metadata.identifier";
    internal const string AppMetadataName = "SDL.app.metadata.name";
    internal const string AppMetadataType = "SDL.app.metadata.type";
    internal const string AppMetadataUrl = "SDL.app.metadata.url";
    internal const string AppMetadataVersion = "SDL.app.metadata.version";

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
