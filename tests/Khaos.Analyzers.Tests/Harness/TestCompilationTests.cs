// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis.CSharp;

namespace KappaDuck.Khaos.Analyzers.Tests.Harness;

internal sealed class TestCompilationTests
{
    [Test]
    public async Task CreateShouldParseWithTheFeaturesOfTheRepository()
    {
        CSharpCompilation compilation = TestCompilation.Create("public sealed class Player;");

        IReadOnlyDictionary<string, string> features = compilation.SyntaxTrees[0].Options.Features;

        await features["runtime-async"].Should().BeEqualTo("on");
        await features["updated-memory-safety-rules"].Should().BeEqualTo("true");
    }

    [Test]
    public async Task EditShouldKeepTheTreesThatDidNotChange()
    {
        CSharpCompilation compilation = TestCompilation.Create("public sealed class Player;", "public sealed class Enemy;");

        CSharpCompilation edited = TestCompilation.Edit(compilation, "public sealed class Player;", "public sealed class Boss;");

        await ReferenceEquals(edited.SyntaxTrees[0], compilation.SyntaxTrees[0]).Should().BeTrue();
        await edited.SyntaxTrees[1].ToString().Should().BeEqualTo("public sealed class Boss;");
        await edited.SyntaxTrees[1].FilePath.Should().BeEqualTo("Source1.cs");
    }

    [Test]
    public async Task EditShouldThrowWhenTheNumberOfSourcesChanges()
    {
        CSharpCompilation compilation = TestCompilation.Create("public sealed class Player;");

        ArgumentException exception = Assert.Throws<ArgumentException>(() => TestCompilation.Edit(compilation, "public sealed class Player;", "public sealed class Enemy;"));

        await exception.ParamName.Should().BeEqualTo("edited");
    }
}
