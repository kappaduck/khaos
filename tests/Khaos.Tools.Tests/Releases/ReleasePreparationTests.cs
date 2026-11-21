// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Releases;
using KappaDuck.Khaos.Tools.Tests.Helpers;

namespace KappaDuck.Khaos.Tools.Tests.Releases;

internal sealed class ReleasePreparationTests : IDisposable
{
    private static readonly DateOnly _releaseDay = new(2026, 11, 20);

    private readonly Workspace _workspace = Workspace.Copy("Releases/input");

    public void Dispose() => _workspace.Dispose();

    [Test]
    public async Task PrepareAsyncShouldDateTheChangelogAddTableRowAndShipTheRules()
    {
        await ReleasePreparation.PrepareAsync(_workspace.Root, _releaseDay);

        await VerifyDirectory(_workspace.Root);
    }

    [Test]
    public async Task PrepareAsyncShouldMarkTheVersion()
    {
        PreparedRelease release = await ReleasePreparation.PrepareAsync(_workspace.Root, _releaseDay);

        await release.Version.Should().BeEqualTo(new Version(0, 1, 0));
    }

    [Test]
    public async Task PrepareAsyncShouldListTheRulesEachAnalyzerShipped()
    {
        PreparedRelease release = await ReleasePreparation.PrepareAsync(_workspace.Root, _releaseDay);

        await release.Analyzers[0].Name.Should().BeEqualTo("analyzers/Quiet");
        await release.Analyzers[0].ShippedRules.Should().BeEmpty();

        await release.Analyzers[1].Name.Should().BeEqualTo("analyzers/Rules");
        await release.Analyzers[1].ShippedRules[0].Should().BeEqualTo("FIX001");
        await release.Analyzers[1].ShippedRules[1].Should().BeEqualTo("FIX002");
    }

    [Test]
    public async Task PrepareAsyncShouldRefuseASecondReleaseOfTheSameVersion()
    {
        await ReleasePreparation.PrepareAsync(_workspace.Root, _releaseDay);

        await Assert.That(async () => await ReleasePreparation.PrepareAsync(_workspace.Root, _releaseDay.AddDays(1)))
                    .ThrowsExactly<InvalidOperationException>();
    }

    [Test]
    public async Task OpenNextAsyncShouldRaiseTheVersionAndOpenTheNextSection()
    {
        await ReleasePreparation.PrepareAsync(_workspace.Root, _releaseDay);

        NextVersion opened = await ReleasePreparation.OpenNextAsync(_workspace.Root, null, 2026);

        await opened.Should().BeEqualTo(new NextVersion(new Version(0, 1, 0), new Version(0, 2, 0)));
        await VerifyDirectory(_workspace.Root);
    }

    [Test]
    public async Task OpenNextAsyncShouldRefuseBeforeTheReleaseIsPrepared()
    {
        await Assert.That(async () => await ReleasePreparation.OpenNextAsync(_workspace.Root, null, 2026))
                    .ThrowsExactly<InvalidOperationException>();
    }

    [Test]
    [Arguments(null, "0.2.0")]
    [Arguments("", "0.2.0")]
    [Arguments("0.1.1", "0.1.1")]
    [Arguments("1.0.0", "1.0.0")]
    public async Task NextVersionShouldTakeTheVersionRequestedOrTheNextMinor(string? requested, string expected)
    {
        Version next = ReleasePreparation.NextVersion(requested, new Version(0, 1, 0));

        await next.Should().BeEqualTo(Version.Parse(expected));
    }

    [Test]
    [Arguments("1.0")]
    [Arguments("1.0.0.0")]
    [Arguments("next")]
    public async Task NextVersionShouldRefuseAVersionThatIsNotMajorMinorPatch(string requested)
    {
        await Assert.That(() => ReleasePreparation.NextVersion(requested, new Version(0, 1, 0)))
                    .ThrowsExactly<FormatException>()
                    .WithMessage($"'{requested}' is not a major.minor.patch version.");
    }
}
