// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Benchmarks;
using KappaDuck.Khaos.Tools.Tests.Helpers;

namespace KappaDuck.Khaos.Tools.Tests.Benchmarks;

internal sealed class ResultsPageTests : IDisposable
{
    private const string Results = "artifacts/benchmarks";
    private const string Page = "# Performance\n\nHow Khaos is measured.\n\n> [!NOTE]\n> The tables appear here once a release is benchmarked.\n";
    private const string Frame = "| Method | Mean |\n|------- |-----:|\n| EmptyFrame | 75 μs |\n";
    private const string Color = "| Method | Mean |\n|------- |-----:|\n| Lerp | 1 ns |\n";
    private const string Comparison = "## Compared with 0.1.0\n\nNo change.\n";

    private readonly Workspace _workspace = Workspace.Empty();
    private readonly string _page;

    public ResultsPageTests() => _page = _workspace.Write(ResultsPage.PagePath, Page);

    public void Dispose() => _workspace.Dispose();

    [Test]
    public async Task UpdateAsyncShouldPutTheReportsUnderTheirHeadingsInPlaceOfTheNote()
    {
        _workspace.Write($"{Results}/KappaDuck.Khaos.Benchmarks.Frames.FrameBenchmarks-report-github.md", Frame);
        _workspace.Write($"{Results}/KappaDuck.Khaos.Benchmarks.Micro.Graphics.ColorBenchmarks-report-github.md", Color);
        _workspace.Write($"{Results}/comparison.md", Comparison);

        await ResultsPage.UpdateAsync(_workspace.Root);

        await VerifyFile(_page);
    }

    [Test]
    public async Task UpdateAsyncShouldLeaveThePageAsItIsWithoutAReport()
    {
        _workspace.Write($"{Results}/comparison.md", Comparison);

        bool updated = await ResultsPage.UpdateAsync(_workspace.Root);

        await updated.Should().BeFalse();
        await VerifyFile(_page);
    }
}
