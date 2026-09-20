// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Tests.Interop;

#pragma warning disable S2930 // "IDisposables" should be disposed
public sealed class OwnedBufferTests
{
    [Test]
    public async Task OwnedBufferShouldWrapThePointer()
    {
        int[] values = [1, 2, 3, 4, 5];

        OwnedBuffer<int> buffer = new(AsPointer(values), values.Length);

        bool matched = buffer.Span.SequenceEqual(values);

        await buffer.Span.Length.Should().BeEqualTo(values.Length);
        await matched.Should().BeTrue();
    }

    [Test]
    public async Task SpanShouldReturnEmptyWhenArrayIsEmpty()
    {
        int[] values = [];
        using OwnedBuffer<int> buffer = new(AsPointer(values), 0);

        await buffer.Span.IsEmpty.Should().BeTrue();
    }

    [Test]
    public async Task SpanShouldReturnEmptyWhenPointerIsNull()
    {
        using OwnedBuffer<int> buffer = new(null, 0);

        await buffer.Span.IsEmpty.Should().BeTrue();
    }

    private static T* AsPointer<T>(ReadOnlySpan<T> array) where T : unmanaged
    {
        return unsafe ((T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(array)));
    }
}
#pragma warning restore S2930 // "IDisposables" should be disposed
