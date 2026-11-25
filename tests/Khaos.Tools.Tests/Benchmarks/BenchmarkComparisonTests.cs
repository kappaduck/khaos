// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Benchmarks;
using KappaDuck.Khaos.Tools.Tests.Helpers;

namespace KappaDuck.Khaos.Tools.Tests.Benchmarks;

internal sealed class BenchmarkComparisonTests
{
    private static readonly ComparisonOptions _options = new(
    Workspace.DataPath("Benchmarks/base.json"),
    Workspace.DataPath("Benchmarks/diff.json"),
    "0.1.0",
    "0.2.0",
    "5%",
    Strict: true);

    [Test]
    public async Task WriteShouldWriteTheExpectedComparison()
    {
        await using StringWriter output = new() { NewLine = "\n" };

        BenchmarkComparison.Write(output, _options);

        await Verify(output.ToString(), "md");
    }

    [Test]
    public async Task WriteShouldCountTheBenchmarkThatAllocatesMore()
    {
        await using StringWriter output = new();

        int allocationsUp = BenchmarkComparison.Write(output, _options);

        await allocationsUp.Should().BeEqualTo(1);
    }

    [Test]
    public async Task WriteShouldFindNoChangeBetweenARunAndItself()
    {
        await using StringWriter output = new() { NewLine = "\n" };

        int allocationsUp = BenchmarkComparison.Write(output, _options with { Diff = _options.Base });

        await allocationsUp.Should().BeZero();
        await output.ToString().Should().Contain("No change beyond 5% since 0.1.0.");
    }
}
