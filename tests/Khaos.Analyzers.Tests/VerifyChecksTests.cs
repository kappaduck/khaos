// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Analyzers.Tests;

internal sealed class VerifyChecksTests
{
    [Test]
    public Task RunShouldFindTheVerifyConventionsInTheRepository()
    {
        return VerifyChecks.Run();
    }
}
