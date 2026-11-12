// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;

namespace KappaDuck.Khaos.Allocations.Tests.Mathematics;

internal sealed class TransformAllocationTests
{
    [Test]
    public async Task TransformRectShouldNotAllocate()
    {
        Transform transform = Transform.Create(new Point(10f, 20f), Angle.FromDegrees(30f), new(2f, 2f));
        Rect rect = new(0f, 0f, 64f, 32f);

        long allocated = Allocated.By((transform, rect), static state => state.transform.TransformRect(state.rect));

        await allocated.Should().BeEqualTo(0L);
    }
}
