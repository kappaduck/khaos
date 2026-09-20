// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL;

internal static partial class SDL3
{
    internal static void Free<TUnmanaged>(TUnmanaged* memory) where TUnmanaged : unmanaged
    {
        if (memory is null)
            return;

        NativeFree(memory);
    }

    internal static void Free<TUnmanaged>(TUnmanaged** memory) where TUnmanaged : unmanaged
    {
        if (memory is null)
            return;

        NativeFree(memory);
    }

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_free")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [UntypedPointer("SDL_free releases any allocation SDL handed to the caller, whatever its element type.")]
    private static safe partial void NativeFree(void* memory);
}
