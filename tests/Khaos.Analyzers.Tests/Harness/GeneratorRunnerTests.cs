// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Tests.Harness.Fixtures;

namespace KappaDuck.Khaos.Analyzers.Tests.Harness;

internal sealed class GeneratorRunnerTests
{
    [Test]
    public async Task RunShouldReturnTheGeneratedSources()
    {
        GeneratorResult result = GeneratorRunner<TypeNameGenerator>.Run("public sealed partial class Player;");

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Source("Player.g.cs").Should().Contain("public const string TypeName = \"Player\";");
    }

    [Test]
    public async Task RunShouldReportTheCompilationErrorsOfTheGeneratedCode()
    {
        GeneratorResult result = GeneratorRunner<TypeNameGenerator>.Run("public sealed class Player;");

        await result.CompilationErrors.Should().Contain("CS0260");
    }

    [Test]
    public async Task RunShouldThrowTheExceptionOfTheGenerator()
    {
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => GeneratorRunner<ThrowingGenerator>.Run("public sealed class Player;"));

        await exception.InnerException!.Message.Should().BeEqualTo(ThrowingGenerator.Message);
    }

    [Test]
    public Task VerifyAsyncShouldSnapshotTheGeneratedSources()
    {
        string[] sources = ["public sealed partial class Player;", "internal sealed partial class Enemy;"];

        return GeneratorRunner<TypeNameGenerator>.VerifyAsync(sources);
    }
}
