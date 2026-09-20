// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.SDL.Marshalling;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Tests.Interop.SDL.Marshalling;

public sealed class BorrowedSpanMarshallerTests
{
    [Test]
    public async Task AllocateContainerForManagedElementsShouldWrapsTheNativeWithoutCopying()
    {
        int[] values = [1, 2, 3, 4, 5];

        ReadOnlySpan<int> marshalled = BorrowedSpanMarshaller<int, int>.AllocateContainerForManagedElements(AsPointer(values), values.Length);

        bool matched = marshalled.SequenceEqual(values);

        await marshalled.Length.Should().BeEqualTo(values.Length);
        await matched.Should().BeTrue();
    }

    [Test]
    public async Task AllocateContainerForManagedElementsShouldReturnEmptyWhenPointerIsNull()
    {
        ReadOnlySpan<int> marshalled = BorrowedSpanMarshaller<int, int>.AllocateContainerForManagedElements(null, 0);

        await marshalled.IsEmpty.Should().BeTrue();
    }

    [Test]
    public async Task AllocateContainerForManagedElementsShouldReturnEmptyWhenArrayIsEmpty()
    {
        int[] values = [];
        ReadOnlySpan<int> marshalled = BorrowedSpanMarshaller<int, int>.AllocateContainerForManagedElements(AsPointer(values), values.Length);

        bool matched = marshalled.SequenceEqual(values);

        await marshalled.Length.Should().BeEqualTo(values.Length);
        await matched.Should().BeTrue();
    }

    [Test]
    public async Task GetManagedValuesDestinationShouldReturnEmptySoNothingIsWrittenInGeneratedCode()
    {
        int[] values = [1, 2, 3, 4, 5];

        ReadOnlySpan<int> marshalled = BorrowedSpanMarshaller<int, int>.AllocateContainerForManagedElements(AsPointer(values), values.Length);

        Span<int> managedValues = BorrowedSpanMarshaller<int, int>.GetManagedValuesDestination(marshalled);

        await managedValues.IsEmpty.Should().BeTrue();
    }

    [Test]
    public async Task GetUnmanagedValuesSourceShouldReturnEmptySoNothingIsWrittenInGeneratedCode()
    {
        int[] values = [1, 2, 3, 4, 5];

        ReadOnlySpan<int> unmanagedValues = BorrowedSpanMarshaller<int, int>.GetUnmanagedValuesSource(AsPointer(values), values.Length);

        await unmanagedValues.IsEmpty.Should().BeTrue();
    }

    private static T* AsPointer<T>(ReadOnlySpan<T> array) where T : unmanaged
    {
        return unsafe ((T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(array)));
    }
}
