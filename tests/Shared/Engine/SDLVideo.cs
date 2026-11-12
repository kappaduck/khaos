// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.SDL;
using KappaDuck.Khaos.Interop.SDL.Marshalling;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace KappaDuck.Khaos.Testing.Engine;

internal static partial class SDLVideo
{
    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetCurrentVideoDriver")]
    [return: MarshalUsing(typeof(BorrowedUtf8StringMarshaller))]
    internal static safe partial string? GetCurrentVideoDriver();

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_ScreenSaverEnabled")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static safe partial bool ScreenSaverEnabled();
}
