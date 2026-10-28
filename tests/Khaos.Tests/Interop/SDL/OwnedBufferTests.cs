// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.SDL;

namespace KappaDuck.Khaos.Tests.Interop.SDL;

internal sealed class OwnedBufferTests
{
    [Test]
    public async Task SpanShouldExposeTheNativeElements()
    {
        int[] expected = [1, 2, 3, 4, 5];
        using OwnedBuffer<int> buffer = AllocateNative(expected);

        bool matched = buffer.Span.SequenceEqual(expected);

        await matched.Should().BeTrue();
    }

    [Test]
    public async Task SpanShouldBeEmptyWhenTheNativeArrayIsEmpty()
    {
        using OwnedBuffer<int> buffer = AllocateNative([]);

        await buffer.Span.IsEmpty.Should().BeTrue();
    }

    [Test]
    public async Task SpanShouldBeEmptyWhenThePointerIsNullWhateverTheLength()
    {
        using OwnedBuffer<int> buffer = new(null, 5);

        await buffer.Span.IsEmpty.Should().BeTrue();
    }

    [Test]
    public async Task ConstructorShouldRejectANegativeLength()
    {
        await Assert.That(static () =>
        {
            using OwnedBuffer<int> buffer = new(SDLMemory.Copy([1, 2, 3]), -1);
        }).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task ConstructorShouldRejectANegativeLengthWhenThePointerIsNull()
    {
        await Assert.That(static () =>
        {
            using OwnedBuffer<int> buffer = new(null, -1);
        }).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task GetEnumeratorShouldVisitEveryElementInOrder()
    {
        int[] expected = [7, 8, 9];
        using OwnedBuffer<int> buffer = AllocateNative(expected);

        List<int> visited = [];
        foreach (int value in buffer)
            visited.Add(value);

        await visited.SequenceEqual(expected).Should().BeTrue();
    }

    private static OwnedBuffer<int> AllocateNative(ReadOnlySpan<int> values) => new(SDLMemory.Copy(values), values.Length);
}
