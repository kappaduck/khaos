// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL;

internal static partial class SDL3
{
    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_CreateProperties")]
    internal static safe partial uint CreateProperties();

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_DestroyProperties")]
    internal static safe partial void DestroyProperties(uint properties);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetNumberProperty", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static safe partial bool SetNumberProperty(uint properties, string name, long value);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetStringProperty", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static safe partial bool SetStringProperty(uint properties, string name, string? value);
}
