// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.SDL;
using KappaDuck.Khaos.Interop.SDL.Marshalling;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace KappaDuck.Khaos.Tests.Helpers;

internal static partial class SDLAppMetadata
{
    internal const string Name = "SDL.app.metadata.name";
    internal const string Version = "SDL.app.metadata.version";
    internal const string Identifier = "SDL.app.metadata.identifier";
    internal const string Creator = "SDL.app.metadata.creator";
    internal const string Copyright = "SDL.app.metadata.copyright";
    internal const string Url = "SDL.app.metadata.url";
    internal const string Type = "SDL.app.metadata.type";

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetAppMetadataProperty", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(BorrowedUtf8StringMarshaller))]
    internal static safe partial string? Get(string name);
}
