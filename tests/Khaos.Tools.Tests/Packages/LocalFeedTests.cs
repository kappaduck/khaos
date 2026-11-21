// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Packages;

namespace KappaDuck.Khaos.Tools.Tests.Packages;

internal sealed class LocalFeedTests
{
    [Test]
    public async Task NuGetConfigShouldHaveLocalFeed()
    {
        string config = LocalFeed.NuGetConfig("directory/folder/khaos/file");

        await Verify(config);
    }
}
