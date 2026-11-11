// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Shared;
using KappaDuck.Khaos.Analyzers.Shared.Unions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace KappaDuck.Khaos.Analyzers.Tests.Shared.Unions;

internal sealed class UnionEmitterTests
{
    private const string Cases = """
        public readonly record struct Quit;
        public readonly record struct Resized(int Width);
        public readonly record struct Dropped(string Path);
        """;

    private static readonly UnionCase _dropped = new("global::Dropped", "Dropped", true);
    private static readonly UnionCase _quit = new("global::Quit", "Quit", false);
    private static readonly UnionCase _resized = new("global::Resized", "Resized", false);

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

        string union = Emit(true, _quit, _resized, _dropped);
        string errors = ErrorsOf(Parsing.Preview, Cases, union, usage);

        await errors.Should().BeEmpty();
    }

    [Test]
    public async Task EmitMembersShouldCompileWithoutTheMemorySafetyRulesWhenFieldsAreNotMarked()
    {
        string union = Emit(false, _quit, _resized, _dropped);

        string errors = ErrorsOf(Parsing.WithoutMemorySafety, Cases, union);

        await errors.Should().BeEmpty();
    }

    [Test]
    public async Task EmitMembersShouldKeepAManagedCaseOutsideTheOverlap()
    {
        string union = Emit(true, _quit, _dropped);

        await union.Should().Contain("    private readonly global::Dropped _droppedCase;\n");
    }

    [Test]
    public async Task EmitMembersShouldLeaveOutTheOverlapWithoutUnmanagedCases()
    {
        string union = Emit(true, _dropped);

        await union.Should().NotContain("LayoutKind.Explicit");
    }

    [Test]
    public async Task EmitMembersShouldLeaveOverlappedFieldsUnmarkedWhenNotAsked()
    {
        string union = Emit(false, _quit);

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

        string union = Emit(true, _quit, _resized, _dropped);

        string warnings = IdsOf(Parsing.Preview, Cases, union, usage);

        await warnings.Should().BeEmpty();
    }

    [Test]
    public async Task EmitMembersShouldMarkOverlappedFieldsSafeWhenAsked()
    {
        string union = Emit(true, _quit);

        await union.Should().Contain("        internal readonly safe global::Quit Quit;\n");
    }

    [Test]
    public Task EmitMembersShouldMatchTheSnapshot()
    {
        string union = Emit(true, _quit, _resized, _dropped);

        return Verify(union, "cs");
    }

    private static string Emit(bool safeField, params ReadOnlySpan<UnionCase> cases)
    {
        SourceBuilder builder = new();

        builder.AppendLine("[global::System.Runtime.CompilerServices.Union]");

        using (builder.AppendBlock("public readonly partial struct Sample"))
        {
            UnionEmitter.EmitMembers(builder, "Sample", cases);
            UnionEmitter.EmitStorage(builder, cases, safeField);
        }

        return builder.Build();
    }

    private static string ErrorsOf(CSharpParseOptions parsing, params ReadOnlySpan<string> sources)
    {
        CSharpCompilation compilation = TestCompilation.Create(parsing, sources);

        return DiagnosticText.Join(compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
    }

    private static string IdsOf(CSharpParseOptions parsing, params ReadOnlySpan<string> sources)
    {
        CSharpCompilation compilation = TestCompilation.Create(parsing, sources);

        return DiagnosticText.Ids(compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity >= DiagnosticSeverity.Warning));
    }
}
