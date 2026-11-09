// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Allocations.Tests.Helpers;
using KappaDuck.Khaos.Hosting;

namespace KappaDuck.Khaos.Allocations.Tests.Hosting;

[Category(Categories.Integration)]
[NotInParallel(Sequential.Engine)]
internal sealed class KhaosEngineAllocationTests
{
    private KhaosEngine _engine = null!;

    [Before(Test)]
    public void StartEngine()
    {
        _engine = KhaosEngine.Start(new SpyGame());
    }

    [After(Test)]
    public void StopEngine()
    {
        _engine.Dispose();
    }

    [Test]
    public async Task IterateShouldNotAllocate()
    {
        long allocated = Allocated.By(_engine, static engine => engine.Iterate());

        await allocated.Should().BeEqualTo(0L);
    }
}
