// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Allocations.Tests.Helpers;
using KappaDuck.Khaos.Mathematics;

namespace KappaDuck.Khaos.Allocations.Tests.Mathematics;

internal sealed class CellEnumeratorAllocationTests
{
    [Test]
    public async Task CellsShouldNotAllocate()
    {
        RectI area = new(3, 5, 40, 23);

        long allocated = Allocated.By(area, static rect =>
        {
            long sum = 0;

            foreach (PointI cell in rect.Cells)
                sum += cell.X + cell.Y;

            return sum;
        });

        await allocated.Should().BeEqualTo(0L);
    }
}
