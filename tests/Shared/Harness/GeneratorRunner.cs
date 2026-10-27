// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Runtime.CompilerServices;

namespace KappaDuck.Khaos.Testing.Harness;

internal static class GeneratorRunner<TGenerator> where TGenerator : IIncrementalGenerator, new()
{
    internal static GeneratorResult Run(params ReadOnlySpan<string> sources)
    {
        GeneratorDriverRunResult result = Generate(TestCompilation.Create(sources), out Compilation output).GetRunResult();

        Dictionary<string, string> generated = result.Results
            .SelectMany(static generator => generator.GeneratedSources)
            .ToDictionary(static file => file.HintName, static file => file.SourceText.ToString(), StringComparer.Ordinal);

        return new GeneratorResult(generated, [.. DiagnosticText.InSourceOrder(result.Diagnostics)], [.. output.GetDiagnostics()]);
    }

    internal static Task Verify(ReadOnlySpan<string> sources, [CallerFilePath] string sourceFile = "")
        => Verifier.Verify(Generate(TestCompilation.Create(sources), out _), sourceFile: sourceFile);

    private static GeneratorDriver Generate(CSharpCompilation compilation, out Compilation output)
    {
        GeneratorDriver driver = CSharpGeneratorDriver
            .Create([new TGenerator().AsSourceGenerator()], parseOptions: Parsing.Preview)
            .RunGeneratorsAndUpdateCompilation(compilation, out output, out _);

        Exception? exception = driver.GetRunResult().Results
            .Select(static generator => generator.Exception)
            .FirstOrDefault(static exception => exception is not null);

        if (exception is not null)
            throw new InvalidOperationException($"{typeof(TGenerator).Name} threw during generation.", exception);

        return driver;
    }
}
