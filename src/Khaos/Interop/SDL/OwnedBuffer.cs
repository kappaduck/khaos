// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL;

[NonCopyable]
internal readonly ref struct OwnedBuffer<T> : IDisposable where T : unmanaged
{
    private readonly T* _pointer;
    private readonly int _length;

    public OwnedBuffer(T* pointer, int length)
    {
        if (length < 0)
            FreeAndThrows(pointer, length);

        _pointer = pointer;
        _length = pointer is null ? 0 : length;
    }

    // SAFETY: SpanOf(in this) ties the span to this buffer, so [UnscopedRef] cannot be dropped without a build error;
    // without it, a span could outlive the using and read the memory after Dispose frees it (use-after-free).
    [UnscopedRef]
    internal ReadOnlySpan<T> Span => SpanOf(in this);

    [UnscopedRef]
    public ReadOnlySpan<T>.Enumerator GetEnumerator() => Span.GetEnumerator();

    public void Dispose() => SDL3.Free(_pointer);

    [DoesNotReturn]
    private static void FreeAndThrows(T* pointer, int length)
    {
        SDL3.Free(pointer);
        throw new ArgumentOutOfRangeException(nameof(length), length, "An owned buffer cannot have a negative length.");
    }

    private static ReadOnlySpan<T> SpanOf(in OwnedBuffer<T> owner)
    {
        if (owner._pointer is null)
            return [];

        return unsafe (new ReadOnlySpan<T>(owner._pointer, owner._length));
    }
}
