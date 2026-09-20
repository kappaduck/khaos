// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL.Marshalling;

[ContiguousCollectionMarshaller]
[CustomMarshaller(typeof(ReadOnlySpan<>), MarshalMode.ManagedToUnmanagedOut, typeof(BorrowedSpanMarshaller<,>))]
internal static class BorrowedSpanMarshaller<T, TUnmanagedElement> where TUnmanagedElement : unmanaged
{
    public static ReadOnlySpan<T> AllocateContainerForManagedElements(TUnmanagedElement* unmanaged, int numElements)
        => unmanaged is null ? [] : unsafe (new ReadOnlySpan<T>(unmanaged, numElements));

    public static Span<T> GetManagedValuesDestination(ReadOnlySpan<T> _) => [];

    public static ReadOnlySpan<TUnmanagedElement> GetUnmanagedValuesSource(TUnmanagedElement* _, int _1) => [];
}
