// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Interop.SDL;

internal static partial class SDL3
{
    [LibraryImport(nameof(SDL3), EntryPoint = "Init")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static partial bool SDL_Init(uint flags);

    [LibraryImport(nameof(SDL3), EntryPoint = "Test")]
    [UntypedPointer("Oh no! :(")]
    internal static partial void Test(void* woah);
}
