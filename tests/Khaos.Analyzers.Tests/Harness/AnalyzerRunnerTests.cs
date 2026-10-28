// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Tests.Harness.Fixtures;

namespace KappaDuck.Khaos.Analyzers.Tests.Harness;

internal sealed class AnalyzerRunnerTests
{
    [Test]
    public async Task RunAsyncShouldReportTheDiagnosticsOfTheAnalyzer()
    {
        AnalyzerResult result = await AnalyzerRunner<TypeAnalyzer>.RunAsync("public sealed class Player;");

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("TEST001");
        await result.Reported.Should().BeEqualTo("Source0.cs(1,21): warning TEST001: Type 'Player' found");
    }

    [Test]
    public async Task RunAsyncShouldThrowTheExceptionOfTheAnalyzer()
    {
        InvalidOperationException? exception = await Assert.ThrowsAsync<InvalidOperationException>(() => AnalyzerRunner<ThrowingAnalyzer>.RunAsync("public sealed class Player;"));

        string? message = exception?.InnerException?.Message;
        await message.Should().BeEqualTo(ThrowingAnalyzer.Message);
    }

    [Test]
    public async Task RunAsyncShouldPassTheOptionsToTheAnalyzer()
    {
        Dictionary<string, string> options = new() { [OptionAnalyzer.Key] = "duck" };

        AnalyzerResult result = await AnalyzerRunner<OptionAnalyzer>.RunAsync(options, "public sealed class Player;");

        await result.Reported.Should().BeEqualTo("warning TEST002: Option is 'duck'");
    }

    [Test]
    public async Task RunAsyncShouldPassNoOptionsByDefault()
    {
        AnalyzerResult result = await AnalyzerRunner<OptionAnalyzer>.RunAsync("public sealed class Player;");

        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public Task VerifyShouldSnapshotTheDiagnostics()
    {
        string[] source = ["public sealed class Player;", "internal enum Team;"];

        return AnalyzerRunner<TypeAnalyzer>.VerifyAsync(source);
    }
}
