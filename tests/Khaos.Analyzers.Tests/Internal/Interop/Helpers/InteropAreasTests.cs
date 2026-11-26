// Copyright (c) KappaDuck.
// Licensed under the MIT license.

extern alias InternalAnalyzers;

using InternalAnalyzers::KappaDuck.Khaos.Internal.Analyzers.Interop.Helpers;

namespace KappaDuck.Khaos.Analyzers.Tests.Internal.Interop.Helpers;

internal sealed class InteropAreasTests
{
    private const InteropArea Unlisted = (InteropArea)99;

    [Test]
    public async Task GetBoolConventionShouldRejectAnUnlistedArea()
    {
        await Assert.That(static () => InteropAreas.GetBoolConvention(Unlisted))
                    .Throws<ArgumentOutOfRangeException>();
    }
}
