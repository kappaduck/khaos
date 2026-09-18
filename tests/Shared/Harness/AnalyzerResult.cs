// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Testing.Harness;

internal sealed record AnalyzerResult(ImmutableArray<Diagnostic> Diagnostics, ImmutableArray<Diagnostic> CompilationDiagnostics)
{
    private const string UnimplementedPartial = "CS8795";

    internal string CompilationErrors => Join(CompilationDiagnostics.Where(IsUnexpectedError));

    internal string Ids => string.Join(", ", Diagnostics.Select(static diagnostic => diagnostic.Id).OrderBy(static id => id, StringComparer.Ordinal));

    internal string Reported => Join(Diagnostics);

    private static bool IsUnexpectedError(Diagnostic diagnostic)
        => diagnostic.Severity == DiagnosticSeverity.Error && !string.Equals(diagnostic.Id, UnimplementedPartial, StringComparison.Ordinal);

    private static string Join(IEnumerable<Diagnostic> diagnostics)
        => string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString()));
}
