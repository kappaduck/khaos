// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Testing.Harness;

internal sealed record AnalyzerResult(ImmutableArray<Diagnostic> Diagnostics, ImmutableArray<Diagnostic> CompilationDiagnostics)
{
    internal string CompilationErrors => DiagnosticText.Join(CompilationDiagnostics.Where(IsUnexpectedError));

    internal string Ids => DiagnosticText.Ids(Diagnostics);

    internal string Reported => DiagnosticText.Join(Diagnostics);

    private static bool IsUnexpectedError(Diagnostic diagnostic)
        => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Id is not ("CS8794" or "CS8795");
}
