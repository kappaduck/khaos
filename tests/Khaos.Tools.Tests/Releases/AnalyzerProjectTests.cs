// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Releases;
using KappaDuck.Khaos.Tools.Tests.Helpers;

namespace KappaDuck.Khaos.Tools.Tests.Releases;

internal sealed class AnalyzerProjectTests : IDisposable
{
    private const string ShippedPath = "analyzers/Rules/AnalyzerReleases.Shipped.md";
    private const string UnshippedPath = "analyzers/Rules/AnalyzerReleases.Unshipped.md";

    private const string Unshipped = """
        ### New Rules

        Rule ID | Category | Severity | Notes
        --------|----------|----------|-------
        FIX001 | Fixture | Error | first rule
        FIX002 | Fixture | Warning | second rule
        """;

    private static readonly Version _first = new(0, 1, 0);

    private readonly Workspace _workspace = Workspace.Empty();
    private readonly string _shippedPath;

    public AnalyzerProjectTests()
    {
        _shippedPath = _workspace.Write(ShippedPath, string.Empty);
        _workspace.Write(UnshippedPath, Unshipped);
    }

    public void Dispose() => _workspace.Dispose();

    [Test]
    public async Task FindAsyncShouldListTheAnalyzersThatTrackTheirReleasesByName()
    {
        _workspace.Write("analyzers/Alpha/AnalyzerReleases.Shipped.md", string.Empty);
        _workspace.Write("analyzers/Alpha/AnalyzerReleases.Unshipped.md", string.Empty);
        _workspace.Write("analyzers/Untracked/readme.md", "# Untracked");

        IReadOnlyList<AnalyzerProject> analyzer = await AnalyzerProject.FindAsync(_workspace.Root);

        await analyzer[0].Name.Should().BeEqualTo("analyzers/Alpha");
        await analyzer[1].Name.Should().BeEqualTo("analyzers/Rules");
    }

    [Test]
    public async Task FindAsyncShouldThrowsWhenDoesNotFoundTheDirectory()
    {
        using Workspace empty = Workspace.Empty();

        await Assert.That(async () => await AnalyzerProject.FindAsync(empty.Root))
                    .ThrowsExactly<DirectoryNotFoundException>();
    }

    [Test]
    public async Task SaveAsyncShouldAddAReleaseSectionToTheShippedFile()
    {
        AnalyzerProject analyzer = await FindAsync();

        analyzer.Ship(_first);
        await analyzer.SaveAsync();

        await VerifyFile(_shippedPath);
    }

    [Test]
    public async Task SaveAsyncShouldStartAnEmptyShippedFileWithTheSection()
    {
        _workspace.Write(ShippedPath, string.Empty);
        AnalyzerProject analyzer = await FindAsync();

        analyzer.Ship(_first);
        await analyzer.SaveAsync();

        await VerifyFile(_shippedPath);
    }

    [Test]
    public async Task SaveAsyncShouldAddTheSectionAfterTheEarlierReleases()
    {
        AnalyzerProject analyzer = await FindAsync();

        analyzer.Ship(_first);
        await analyzer.SaveAsync();

        _workspace.Write(UnshippedPath, Unshipped.Replace("FIX00", "FIX01", StringComparison.Ordinal));

        AnalyzerProject another = await FindAsync();

        another.Ship(new Version(0, 2, 0));
        await another.SaveAsync();

        await VerifyFile(_shippedPath);
    }

    [Test]
    public async Task SaveAsyncShouldLeaveTheFilesAsTheyWereWithoutAnUnshippedRule()
    {
        _workspace.Write(UnshippedPath, string.Empty);

        AnalyzerProject analyzer = await FindAsync();

        analyzer.Ship(_first);
        await analyzer.SaveAsync();

        await analyzer.ShippedRules.Count.Should().BeZero();
    }

    [Test]
    public async Task ShipShouldListTheIdsOfTheRulesItMoves()
    {
        AnalyzerProject analyzer = await FindAsync();

        analyzer.Ship(_first);

        await analyzer.ShippedRules[0].Should().BeEqualTo("FIX001");
        await analyzer.ShippedRules[1].Should().BeEqualTo("FIX002");
    }

    [Test]
    public async Task ShipShouldRefuseASecondSectionForTheSameVersion()
    {
        AnalyzerProject analyzer = await FindAsync();

        analyzer.Ship(_first);
        await analyzer.SaveAsync();

        _workspace.Write(UnshippedPath, Unshipped);

        AnalyzerProject again = await FindAsync();

        await Assert.That(() => again.Ship(_first))
                    .ThrowsExactly<InvalidOperationException>();
    }

    private async Task<AnalyzerProject> FindAsync() => (await AnalyzerProject.FindAsync(_workspace.Root))[0];
}
