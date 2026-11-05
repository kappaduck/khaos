// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Internal.Analyzers.Tests.Interop;

internal sealed class InteropAreasTests
{
    private const InteropArea Unlisted = (InteropArea)99;

    [Test]
    public async Task BoolConventionShouldRejectAnUnlistedArea()
    {
        await Assert.That(static () => InteropAreas.BoolConvention(Unlisted)).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task BoolStorageShouldRejectAnUnlistedArea()
    {
        await Assert.That(static () => InteropAreas.BoolStorage(Unlisted)).Throws<ArgumentOutOfRangeException>();
    }
}
