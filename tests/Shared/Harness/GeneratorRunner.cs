// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace KappaDuck.Khaos.Testing.Harness;

internal static class GeneratorRunner<TGenerator> where TGenerator : IIncrementalGenerator, new()
{
    internal static GeneratorResult Run(params string[] sources)
    {
        GeneratorDriver driver = Create();
        driver = driver.RunGeneratorsAndUpdateCompilation(TestCompilation.Create(sources), out Compilation output, out _);

        GeneratorDriverRunResult result = driver.GetRunResult();

        ImmutableArray<Diagnostic> generatorDiagnostics = [.. result.Results.SelectMany(static generator => generator.Diagnostics)];
        ImmutableArray<Diagnostic> compilationDiagnostics = [.. output.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)];

        Dictionary<string, string> generated = result.Results
            .SelectMany(static generator => generator.GeneratedSources)
            .ToDictionary(static file => file.HintName, static file => file.SourceText.ToString(), StringComparer.Ordinal);

        return new GeneratorResult(generated, generatorDiagnostics, compilationDiagnostics);
    }

    internal static Task Verify(string[] sources, [CallerFilePath] string sourceFile = "")
        => Verifier.Verify(Create().RunGenerators(TestCompilation.Create(sources)), sourceFile: sourceFile);

    private static CSharpGeneratorDriver Create()
        => CSharpGeneratorDriver.Create([new TGenerator().AsSourceGenerator()], parseOptions: Parsing.Preview);
}
