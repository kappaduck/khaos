// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Tests.Interop.SDL;

internal static partial class SDLMemory
{
    internal static T* Copy<T>(ReadOnlySpan<T> values) where T : unmanaged
    {
        unsafe
        {
            T* memory = (T*)Malloc((nuint)(values.Length * sizeof(T)));

            if (memory is null)
                throw new InvalidOperationException($"SDL_malloc could not allocate {values.Length} elements of {typeof(T).Name}.");

            values.CopyTo(new Span<T>(memory, values.Length));
            return memory;
        }
    }

    [LibraryImport("SDL3", EntryPoint = "SDL_malloc")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial void* Malloc(nuint size);
}
