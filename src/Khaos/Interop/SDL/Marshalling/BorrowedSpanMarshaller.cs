// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL.Marshalling;

[ContiguousCollectionMarshaller]
[CustomMarshaller(typeof(ReadOnlySpan<>), MarshalMode.ManagedToUnmanagedOut, typeof(BorrowedSpanMarshaller<,>))]
internal static class BorrowedSpanMarshaller<T, TUnmanagedElement> where T : unmanaged where TUnmanagedElement : unmanaged
{
    public static ReadOnlySpan<T> AllocateContainerForManagedElements(TUnmanagedElement* unmanaged, int numElements)
    {
        if (sizeof(T) != sizeof(TUnmanagedElement))
            throw new InvalidOperationException($"A borrowed span reads the native memory as it is, so {typeof(T).Name} must have the size of {typeof(TUnmanagedElement).Name}.");

        if (unmanaged is null)
            return [];

        // SAFETY: SDL keeps ownership of the numElements elements at unmanaged and documents how long they stay valid; T has the same size, so the span reads them as they are.
        return unsafe (new ReadOnlySpan<T>(unmanaged, numElements));
    }

    public static ReadOnlySpan<TUnmanagedElement> GetUnmanagedValuesSource(TUnmanagedElement* _, int _1) => [];

    public static Span<T> GetManagedValuesDestination(ReadOnlySpan<T> _) => [];
}
