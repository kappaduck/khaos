// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop;

internal readonly ref struct OwnedBuffer<T>(T* pointer, int length) : IDisposable where T : unmanaged
{
    internal ReadOnlySpan<T> Span => pointer is null ? [] : unsafe (new ReadOnlySpan<T>(pointer, length));

    public void Dispose()
    {
        if (pointer is null || length == 0)
            return;

        SDL3.Free(pointer);
    }
}
