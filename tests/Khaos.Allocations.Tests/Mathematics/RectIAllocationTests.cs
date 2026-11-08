// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Allocations.Tests.Helpers;
using KappaDuck.Khaos.Mathematics;
using System.Globalization;

namespace KappaDuck.Khaos.Allocations.Tests.Mathematics;

internal sealed class RectIAllocationTests
{
    [Test]
    public async Task ContainsAllShouldNotAllocate()
    {
        RectI rect = new(0, 0, 64, 64);
        PointI[] points = [new(1, 2), new(30, 4), new(5, 60), new(63, 63)];

        long allocated = Allocated.By((rect, points), static state => state.rect.ContainsAll(state.points.AsSpan()));

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task ContainsAllShouldNotAllocateGivenAnArrayAsASequence()
    {
        RectI rect = new(0, 0, 64, 64);
        IEnumerable<PointI> points = (PointI[])[new(1, 2), new(30, 4), new(5, 60), new(63, 63)];

        long allocated = Allocated.By((rect, points), static state => state.rect.ContainsAll(state.points));

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task TryEncloseShouldNotAllocate()
    {
        PointI[] points = [new(1, 2), new(30, 4), new(5, 60), new(-7, 8)];

        long allocated = Allocated.By(points, static corners => RectI.TryEnclose(corners.AsSpan(), out _));

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task TryFormatShouldNotAllocate()
    {
        char[] buffer = new char[64];
        RectI rect = new(-12, 48, 1280, 720);

        long allocated = Allocated.By((rect, buffer), static state =>
            state.rect.TryFormat(state.buffer, out _, default, CultureInfo.InvariantCulture));

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task TryIntersectShouldNotAllocate()
    {
        RectI left = new(0, 0, 10, 10);
        RectI right = new(5, 5, 10, 10);

        long allocated = Allocated.By((left, right), static state => RectI.TryIntersect(state.left, state.right, out _));

        await allocated.Should().BeEqualTo(0L);
    }
}
