// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Shared;
using KappaDuck.Khaos.Analyzers.Shared.Unions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace KappaDuck.Khaos.Analyzers.Tests.Shared.Unions;

internal sealed class UnionEmitterTests : IDisposable
{
    private const string Cases = """
        public readonly record struct Quit;
        public readonly record struct Resized(int Width);
        public readonly record struct Dropped(string Path);
        """;

    private static readonly UnionCase _dropped = new("global::Dropped", "Dropped", true);
    private static readonly UnionCase _quit = new("global::Quit", "Quit", false);
    private static readonly UnionCase _resized = new("global::Resized", "Resized", false);

    private readonly CodeSourceBuilder _builder = new();

    public void Dispose() => _builder.Dispose();

    [Test]
    public async Task EmitMembersShouldCompileForUnmanagedAndManagedCases()
    {
        const string usage = """
            public static class Usage
            {
                public static int Match(in Sample sample) => sample switch
                {
                    Quit => 1,
                    Resized { Width: 3 } => 2,
                    Dropped dropped => dropped.Path.Length,
                    _ => 0
                };
            }
            """;

        UnionCase[] unionCases = [_quit, _resized, _dropped];

        _builder.AppendLine("[global::System.Runtime.CompilerServices.Union]");

        using (_builder.AppendBlock("public readonly partial struct Sample"))
        {
            UnionEmitter.EmitMembers(_builder, "Sample", unionCases);
            UnionEmitter.EmitStorage(_builder, unionCases, true);
        }

        string union = _builder.ToString();

        CSharpCompilation compilation = TestCompilation.Create(Parsing.Preview, Cases, union, usage);
        string errors = DiagnosticText.Join(compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

        await errors.Should().BeEmpty();
    }

    [Test]
    public async Task EmitMembersShouldCompileWithoutTheMemorySafetyRulesWhenFieldsAreNotMarked()
    {
        UnionCase[] unionCases = [_quit, _resized, _dropped];

        _builder.AppendLine("[global::System.Runtime.CompilerServices.Union]");

        using (_builder.AppendBlock("public readonly partial struct Sample"))
        {
            UnionEmitter.EmitMembers(_builder, "Sample", unionCases);
            UnionEmitter.EmitStorage(_builder, unionCases, false);
        }

        string union = _builder.ToString();

        CSharpCompilation compilation = TestCompilation.Create(Parsing.WithoutMemorySafety, Cases, union);
        string errors = DiagnosticText.Join(compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

        await errors.Should().BeEmpty();
    }

    [Test]
    public async Task EmitMembersShouldKeepAManagedCaseOutsideTheOverlap()
    {
        UnionCase[] unionCases = [_quit, _dropped];

        _builder.AppendLine("[global::System.Runtime.CompilerServices.Union]");

        using (_builder.AppendBlock("public readonly partial struct Sample"))
        {
            UnionEmitter.EmitMembers(_builder, "Sample", unionCases);
            UnionEmitter.EmitStorage(_builder, unionCases, true);
        }

        string union = _builder.ToString();

        await union.Should().Contain("    private readonly global::Dropped _droppedCase;\n");
    }

    [Test]
    public async Task EmitMembersShouldLeaveOutTheOverlapWithoutUnmanagedCases()
    {
        UnionCase[] unionCases = [_dropped];

        _builder.AppendLine("[global::System.Runtime.CompilerServices.Union]");

        using (_builder.AppendBlock("public readonly partial struct Sample"))
        {
            UnionEmitter.EmitMembers(_builder, "Sample", unionCases);
            UnionEmitter.EmitStorage(_builder, unionCases, true);
        }

        string union = _builder.ToString();

        await union.Should().NotContain("LayoutKind.Explicit");
    }

    [Test]
    public async Task EmitMembersShouldLeaveOverlappedFieldsUnmarkedWhenNotAsked()
    {
        UnionCase[] unionCases = [_quit];

        _builder.AppendLine("[global::System.Runtime.CompilerServices.Union]");

        using (_builder.AppendBlock("public readonly partial struct Sample"))
        {
            UnionEmitter.EmitMembers(_builder, "Sample", unionCases);
            UnionEmitter.EmitStorage(_builder, unionCases, false);
        }

        string union = _builder.ToString();

        await union.Should().Contain("        internal readonly global::Quit Quit;\n");
    }

    [Test]
    public async Task EmitMembersShouldMakeASwitchWithANullArmExhaustive()
    {
        const string usage = """
            public static class Usage
            {
                public static int Match(in Sample sample) => sample switch
                {
                    Quit => 1,
                    Resized => 2,
                    Dropped => 3,
                    null => 0
                };
            }
            """;

        UnionCase[] unionCases = [_quit, _resized, _dropped];

        _builder.AppendLine("[global::System.Runtime.CompilerServices.Union]");

        using (_builder.AppendBlock("public readonly partial struct Sample"))
        {
            UnionEmitter.EmitMembers(_builder, "Sample", unionCases);
            UnionEmitter.EmitStorage(_builder, unionCases, true);
        }

        string union = _builder.ToString();

        CSharpCompilation compilation = TestCompilation.Create(Parsing.Preview, Cases, union, usage);
        string warnings = DiagnosticText.Ids(compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity >= DiagnosticSeverity.Warning));

        await warnings.Should().BeEmpty();
    }

    [Test]
    public async Task EmitMembersShouldMarkOverlappedFieldsSafeWhenAsked()
    {
        UnionCase[] unionCases = [_quit];

        _builder.AppendLine("[global::System.Runtime.CompilerServices.Union]");

        using (_builder.AppendBlock("public readonly partial struct Sample"))
        {
            UnionEmitter.EmitMembers(_builder, "Sample", unionCases);
            UnionEmitter.EmitStorage(_builder, unionCases, true);
        }

        string union = _builder.ToString();

        await union.Should().Contain("        internal readonly safe global::Quit Quit;\n");
    }

    [Test]
    public Task EmitMembersShouldMatchTheSnapshot()
    {
        UnionCase[] unionCases = [_quit, _resized, _dropped];

        _builder.AppendLine("[global::System.Runtime.CompilerServices.Union]");

        using (_builder.AppendBlock("public readonly partial struct Sample"))
        {
            UnionEmitter.EmitMembers(_builder, "Sample", unionCases);
            UnionEmitter.EmitStorage(_builder, unionCases, true);
        }

        string union = _builder.ToString();

        return Verify(union, "cs");
    }

    [Test]
    public async Task CrefShouldDropTheGlobalPrefix()
    {
        string cref = UnionEmitter.Cref("global::MyGame.ScoreChanged");

        await cref.Should().BeEqualTo("MyGame.ScoreChanged");
    }

    [Test]
    public async Task CrefShouldKeepANameWithoutThePrefix()
    {
        string cref = UnionEmitter.Cref("MyGame.ScoreChanged");

        await cref.Should().BeEqualTo("MyGame.ScoreChanged");
    }
}
