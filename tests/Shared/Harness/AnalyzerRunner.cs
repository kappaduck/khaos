// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace KappaDuck.Khaos.Testing.Harness;

internal static class AnalyzerRunner<TAnalyzer> where TAnalyzer : DiagnosticAnalyzer, new()
{
    internal static Task<AnalyzerResult> RunAsync(params ReadOnlySpan<string> sources)
        => RunAsync(ImmutableDictionary<string, string>.Empty, sources);

    internal static Task<AnalyzerResult> RunAsync(IReadOnlyDictionary<string, string> options, params ReadOnlySpan<string> sources)
        => AnalyzeAsync(options, TestCompilation.Create(sources));

    internal static Task VerifyAsync(ReadOnlySpan<string> sources, [CallerFilePath] string sourceFile = "")
        => VerifyAsync(TestCompilation.Create(sources), sourceFile);

    private static async Task<AnalyzerResult> AnalyzeAsync(IReadOnlyDictionary<string, string> options, CSharpCompilation compilation)
    {
        ConcurrentQueue<Exception> exceptions = new();

        CompilationWithAnalyzersOptions analysis = new(
            new AnalyzerOptions([], new AnalyzerOptionsProvider(options)),
            (exception, _, _) => exceptions.Enqueue(exception),
            concurrentAnalysis: true,
            logAnalyzerExecutionTime: false
        );

        ImmutableArray<Diagnostic> reported = await compilation.WithAnalyzers([new TAnalyzer()], analysis).GetAnalyzerDiagnosticsAsync();

        if (exceptions.TryPeek(out Exception? exception))
            throw new InvalidOperationException($"{typeof(TAnalyzer).Name} threw during analysis.", exception);

        return new AnalyzerResult([.. DiagnosticText.InSourceOrder(reported)], [.. compilation.GetDiagnostics()]);
    }

    private static async Task VerifyAsync(CSharpCompilation compilation, [CallerFilePath] string sourceFile = "")
    {
        AnalyzerResult result = await AnalyzeAsync(ImmutableDictionary<string, string>.Empty, compilation);

        if (result.CompilationErrors.Length > 0)
            throw new InvalidOperationException($"The test sources do not compile:{Environment.NewLine}{result.CompilationErrors}");

        await Verify(result.Diagnostics, sourceFile: sourceFile);
    }
}
