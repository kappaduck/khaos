// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL;

internal static partial class SDL3
{
    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_PushEvent")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static safe partial bool PushEvent(ref SDL_Event e);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_RegisterEvents")]
    internal static safe partial uint RegisterEvents(int count);
}
