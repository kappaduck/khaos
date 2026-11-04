// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL;

[NonCopyable]
internal readonly ref struct OwnedBuffer<T> : IDisposable where T : unmanaged
{
    private readonly T* _pointer;

    public OwnedBuffer(T* pointer, int length)
    {
        if (length < 0)
            FreeAndThrows(pointer, length);

        _pointer = pointer;
        Length = pointer is null ? 0 : length;
    }

    internal int Length { get; }

    internal T this[int index] => Span[index];

    // SAFETY: SpanOf(in this) ties the span to this buffer, so [UnscopedRef] cannot be dropped without a build error;
    // without it, a span could outlive the using and read the memory after Dispose frees it (use-after-free).
    [UnscopedRef]
    internal ReadOnlySpan<T> Span => SpanOf(in this);

    public void Dispose() => SDL3.Free(_pointer);

    [UnscopedRef]
    public ReadOnlySpan<T>.Enumerator GetEnumerator() => Span.GetEnumerator();

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

        return unsafe (new ReadOnlySpan<T>(owner._pointer, owner.Length));
    }
}
