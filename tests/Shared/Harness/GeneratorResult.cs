// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Testing.Harness;

internal sealed record GeneratorResult(IReadOnlyDictionary<string, string> GeneratedFiles, ImmutableArray<Diagnostic> GeneratorDiagnostics, ImmutableArray<Diagnostic> CompilationDiagnostics)
{
    internal string CompilationErrors => string.Join(Environment.NewLine, CompilationDiagnostics.Select(static diagnostic => diagnostic.ToString()));

    internal string GeneratorDiagnosticIds => string.Join(", ", GeneratorDiagnostics.Select(static diagnostic => diagnostic.Id).OrderBy(static id => id, StringComparer.Ordinal));

    internal string Source(string hintName) => GeneratedFiles.TryGetValue(hintName, out string? value) ? value : string.Empty;
}
