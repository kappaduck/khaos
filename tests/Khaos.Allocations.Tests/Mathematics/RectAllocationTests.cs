// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;
using System.Globalization;

namespace KappaDuck.Khaos.Allocations.Tests.Mathematics;

internal sealed class RectAllocationTests
{
    [Test]
    public async Task TryFormatShouldNotAllocate()
    {
        char[] buffer = new char[64];
        Rect rect = new(1.5f, 2.5f, 3.5f, 4.5f);

        long allocated = Allocated.By((rect, buffer), static state => state.rect.TryFormat(state.buffer, out _, default, CultureInfo.InvariantCulture));

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task ContainsAllShouldNotAllocateGivenAnArrayAsASequence()
    {
        Rect rect = new(0f, 0f, 64f, 64f);
        IEnumerable<Point> points = (Point[])[new(1f, 2f), new(30f, 4f), new(5f, 60f), new(64f, 64f)];

        long allocated = Allocated.By((rect, points), static state => state.rect.ContainsAll(state.points));

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task TryEncloseShouldNotAllocate()
    {
        Point[] points = [new(1f, 2f), new(30f, 4f), new(5f, 60f), new(-7f, 8f)];

        long allocated = Allocated.By(points, static corners => Rect.TryEnclose(corners.AsSpan(), out _));

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task TryIntersectShouldNotAllocate()
    {
        Rect left = new(0f, 0f, 10f, 10f);
        Rect right = new(5f, 5f, 10f, 10f);

        long allocated = Allocated.By((left, right), static state => Rect.TryIntersect(state.left, state.right, out _));

        await allocated.Should().BeEqualTo(0L);
    }
}
