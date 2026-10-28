// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Testing.Harness;

internal sealed record GeneratorResult(IReadOnlyDictionary<string, string> GeneratedFiles, ImmutableArray<Diagnostic> Diagnostics, ImmutableArray<Diagnostic> CompilationDiagnostics)
{
    internal string CompilationErrors => DiagnosticText.Join(CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

    internal string Ids => DiagnosticText.Ids(Diagnostics);

    internal string Reported => DiagnosticText.Join(Diagnostics);

    internal string Source(string hintName) => GeneratedFiles.GetValueOrDefault(hintName, string.Empty);
}
