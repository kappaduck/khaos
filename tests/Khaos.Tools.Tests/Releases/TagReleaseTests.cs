// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Releases;

namespace KappaDuck.Khaos.Tools.Tests.Releases;

internal sealed class TagReleaseTests
{
    private static readonly string[] _tags = ["v0.1.0", "v0.1.1", "v0.2.0", "v0.3.0-beta.1"];

    [Test]
    [Arguments("beta", ReleaseChannel.Beta)]
    [Arguments("prod", ReleaseChannel.Release)]
    [Arguments("BETA", ReleaseChannel.Beta)]
    [Arguments("PROD", ReleaseChannel.Release)]
    [Arguments("Beta", ReleaseChannel.Beta)]
    [Arguments("Other", ReleaseChannel.Release)]
    public async Task ChannelOfShouldReturnTheGoodChannel(string name, ReleaseChannel expected)
    {
        ReleaseChannel channel = TagRelease.ChannelOf(name);

        await channel.Should().BeEqualTo(expected);
    }

    [Test]
    [Arguments("v0.1.0", "0.1.0")]
    [Arguments("0.1.0", "0.1.0")]
    [Arguments("V0.1.0", "0.1.0")]
    public async Task NormalizeShouldTheTag(string tag, string expected)
    {
        string normalized = TagRelease.Normalize(tag);

        await normalized.Should().BeEqualTo(expected);
    }

    [Test]
    public async Task LatestShouldFindTheHighestStableVersion()
    {
        LatestRelease latest = TagRelease.Latest("v0.2.0", _tags);

        await latest.Highest.Should().BeEqualTo(new Version(0, 2, 0));
    }

    [Test]
    public async Task LatestShouldFindThePreviousStableVersion()
    {
        LatestRelease latest = TagRelease.Latest("v0.2.0", _tags);

        await latest.Previous.Should().BeEqualTo("v0.1.1");
    }

    [Test]
    public async Task LatestShouldFindTheLatestStableVersion()
    {
        LatestRelease latest = TagRelease.Latest("v0.2.0", ["v0.2.0", "v0.1.0"]);

        await latest.IsLatest.Should().BeTrue();
    }

    [Test]
    public async Task LatestShouldFindTheLatestStableVersionEvenIsContainsBetaVersion()
    {
        LatestRelease latest = TagRelease.Latest("v0.2.0", ["v0.2.0", "v0.1.0", "v0.3.0-beta.1"]);

        await latest.IsLatest.Should().BeTrue();
    }

    [Test]
    public async Task LatestShouldReturnFalseWhenTagIsOlderThanTheLatest()
    {
        LatestRelease latest = TagRelease.Latest("v0.1.0", ["v0.2.0", "v0.1.0"]);

        await latest.IsLatest.Should().BeFalse();
    }

    [Test]
    public async Task LatestShouldGiveNoPreviousTagToTheFirstRelease()
    {
        LatestRelease latest = TagRelease.Latest("v0.1.0", ["v0.1.0"]);

        await latest.Previous.Should().BeNull();
    }

    [Test]
    public async Task LatestShouldNeverTakeABetaAsTheHighest()
    {
        LatestRelease latest = TagRelease.Latest("v0.2.0", ["v0.2.0", "v0.10.0-beta.1"]);

        await latest.Highest.Should().BeEqualTo(new Version(0, 2, 0));
    }

    [Test]
    public async Task LatestShouldHaveNoHighestWhenThereIsNoStableVersion()
    {
        LatestRelease latest = TagRelease.Latest("v0.2.0", ["v0.2.0-beta.2", "v0.10.0-beta.1"]);

        await latest.Highest.Should().BeNull();
    }

    [Test]
    [Arguments("v0.1.1", "0.1")]
    [Arguments("v1.2.0", "1.2")]
    [Arguments("0.3.4", "0.3")]
    public async Task LineOfShouldDropThePatch(string tag, string expected)
    {
        string line = TagRelease.LineOf(tag);

        await line.Should().BeEqualTo(expected);
    }

    [Test]
    [Arguments("v0.1.0", ReleaseChannel.Release)]
    [Arguments("0.1.0", ReleaseChannel.Release)]
    [Arguments("v0.1.0-beta.3", ReleaseChannel.Beta)]
    public async Task MatchesShouldTakeTheTagOfThePrefix(string tag, ReleaseChannel channel)
    {
        bool matches = TagRelease.Matches(tag, "0.1.0", channel);

        await matches.Should().BeTrue();
    }

    [Test]
    [Arguments("v0.1.1", ReleaseChannel.Release)]
    [Arguments("v0.1.0-beta.3", ReleaseChannel.Release)]
    [Arguments("v0.1.0", ReleaseChannel.Beta)]
    [Arguments("v0.2.0-beta.1", ReleaseChannel.Beta)]
    [Arguments("v0.1.0-beta.x", ReleaseChannel.Beta)]
    public async Task MatchesShouldRefuseAnotherTag(string tag, ReleaseChannel channel)
    {
        bool matches = TagRelease.Matches(tag, "0.1.0", channel);

        await matches.Should().BeFalse();
    }

    [Test]
    [Arguments("v0.1.0", ReleaseChannel.Release, new string[] { "0.1.0-beta.3" }, "0.1.0", null)]
    [Arguments(null, ReleaseChannel.Beta, new string[] { }, "0.1.0-beta.1", "beta.1")]
    [Arguments(null, ReleaseChannel.Beta, new string[] { "0.1.0-beta.9", "0.1.0-beta.10" }, "0.1.0-beta.11", "beta.11")]
    [Arguments(null, ReleaseChannel.Beta, new string[] { "0.1.0", "0.2.0-beta.4", "0.10.0-beta.7" }, "0.1.0-beta.1", "beta.1")]
    [Arguments("v0.1.0-beta.3", ReleaseChannel.Beta, new string[] { "0.1.0-beta.1", "0.1.0-beta.2" }, "0.1.0-beta.3", "beta.3")]
    [Arguments("v0.1.0-beta.5", ReleaseChannel.Beta, new string[] { "0.1.0-beta.2" }, "0.1.0-beta.5", "beta.5")]
    public async Task ResolveShouldGiveTheVersionToPack(string? tag, ReleaseChannel channel, string[] published, string version, string? suffix)
    {
        VersionResolution resolution = TagRelease.Resolve("0.1.0", tag, channel, published);

        ResolvedVersion resolved = (ResolvedVersion)resolution.Value;

        await resolved.Version.Should().BeEqualTo(version);
        await resolved.Suffix.Should().BeEqualTo(suffix);
    }

    [Test]
    [Arguments("v0.1.0-beta.3", new string[] { "0.1.0-beta.2", "0.1.0-beta.3" }, "The tag asks for 0.1.0-beta.3, but NuGet already has 0.1.0-beta.3. Tag the pre-release v0.1.0-beta.4.")]
    [Arguments("v0.1.0-beta.3", new string[] { "0.1.0-beta.4" }, "The tag asks for 0.1.0-beta.3, but NuGet already has 0.1.0-beta.4. Tag the pre-release v0.1.0-beta.5.")]
    [Arguments("v0.2.0-beta.1", new string[] { }, "The tag v0.2.0-beta.1 is not a beta of 0.1.0. Tag the pre-release v0.1.0-beta.1.")]
    public async Task ResolveShouldRefuseATagNuGetCannotTake(string tag, string[] published, string error)
    {
        VersionResolution resolution = TagRelease.Resolve("0.1.0", tag, ReleaseChannel.Beta, published);

        string refused = (string)resolution.Value;

        await refused.Should().BeEqualTo(error);
    }
}
