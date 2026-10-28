// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL;

internal static partial class SDL3
{
    internal static void Free<T>(T* memory) where T : unmanaged
    {
        if (memory is null)
            return;

        // SAFETY: the caller vouches that SDL allocated memory and that nothing frees it a second time.
        unsafe
        {
            NativeFree(memory);
        }
    }

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_free")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [UntypedBecause("SDL_free releases any allocation SDL handed out, whatever its element type")]
    private static unsafe partial void NativeFree(void* memory);
}
