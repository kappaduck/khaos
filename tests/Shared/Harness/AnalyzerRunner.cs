// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Testing.Harness;

internal static class AnalyzerRunner<TAnalyzer> where TAnalyzer : DiagnosticAnalyzer, new()
{
    private const string AnalyzerThrew = "AD0001";

    internal static async Task<AnalyzerResult> RunAsync(params string[] sources)
    {
        CSharpCompilation compilation = TestCompilation.Create(sources);

        ImmutableArray<Diagnostic> reported = await compilation.WithAnalyzers([new TAnalyzer()]).GetAnalyzerDiagnosticsAsync();

        Diagnostic? crash = reported.FirstOrDefault(static diagnostic => string.Equals(diagnostic.Id, AnalyzerThrew, StringComparison.Ordinal));

        if (crash is not null)
            throw new InvalidOperationException($"{typeof(TAnalyzer).Name} threw during analysis: {crash}");

        return new AnalyzerResult(reported, [.. compilation.GetDiagnostics()]);
    }
}
