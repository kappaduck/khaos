// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Allocations.Tests.Helpers;

namespace KappaDuck.Khaos.Allocations.Tests;

[Category(Categories.Integration)]
[NotInParallel(Sequential.Engine)]
internal sealed class Game2DAllocationTests
{
    private const int FirstMeasuredFrame = 10;
    private const int LastMeasuredFrame = 110;

    [Test]
    public async Task RunShouldNotAllocateBetweenFrames()
    {
        long[] allocated = new long[2];
        SpyGame game = new() { OnDraw = game => Measure(game, allocated) };

        game.Run();

        await allocated[1].Should().BeEqualTo(allocated[0]);
    }

    private static void Measure(SpyGame game, long[] allocated)
    {
        if (game.Draws == FirstMeasuredFrame)
            allocated[0] = GC.GetAllocatedBytesForCurrentThread();

        if (game.Draws != LastMeasuredFrame)
            return;

        allocated[1] = GC.GetAllocatedBytesForCurrentThread();
        game.Exit();
    }
}
