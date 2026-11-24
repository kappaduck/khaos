// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Benchmarks;

namespace KappaDuck.Khaos.Tools.Tests.Benchmarks;

internal sealed class ComparisonOptionsTests
{
    [Test]
    public async Task TryParseShouldReturnTrueWithTheBaseAndTheDiff()
    {
        string[] args = ["--base", "a", "--diff", "b"];

        bool parsed = ComparisonOptions.TryParse(args, out _);

        await parsed.Should().BeTrue();
    }

    [Test]
    public async Task TryParseShouldGiveTheDefaultsOfTheOtherOptions()
    {
        string[] args = ["--base", "a", "--diff", "b"];

        _ = ComparisonOptions.TryParse(args, out ComparisonOptions? options);

        await options.Should().BeEqualTo(new ComparisonOptions("a", "b", "base", "diff", "5%", Strict: false));
    }

    [Test]
    public async Task TryParseShouldReadEveryOption()
    {
        string[] args = ["--base", "a", "--diff", "b", "--base-name", "0.1.0", "--diff-name", "0.2.0", "--threshold", "10%", "--strict"];

        _ = ComparisonOptions.TryParse(args, out ComparisonOptions? options);

        await options.Should().BeEqualTo(new ComparisonOptions("a", "b", "0.1.0", "0.2.0", "10%", Strict: true));
    }

    [Test]
    public async Task TryParseShouldReadStrictBeforeTheOtherOptions()
    {
        string[] args = ["--strict", "--base", "a", "--diff", "b"];

        _ = ComparisonOptions.TryParse(args, out ComparisonOptions? options);

        await options.Should().BeEqualTo(new ComparisonOptions("a", "b", "base", "diff", "5%", Strict: true));
    }

    [Test]
    [Arguments("--base a")]
    [Arguments("--diff b")]
    [Arguments("--base a --diff")]
    [Arguments("--base a --diff b --unknown c")]
    public async Task TryParseShouldReturnFalseWithIncompleteArguments(string args)
    {
        bool parsed = ComparisonOptions.TryParse(args.Split(' '), out ComparisonOptions? options);

        await parsed.Should().BeFalse();
        await options.Should().BeNull();
    }
}
