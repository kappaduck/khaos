// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Runtime.CompilerServices;

namespace KappaDuck.Khaos.Analyzers.Tests.Helpers;

internal static class GeneratorRunner<TGenerator> where TGenerator : IIncrementalGenerator, new()
{
    internal static GeneratorResult Run(params ReadOnlySpan<string> sources) => Run(Parsing.Preview, sources);

    internal static GeneratorResult Run(CSharpParseOptions parsing, params ReadOnlySpan<string> sources) => Run(TestCompilation.Create(parsing, sources));

    internal static GeneratorResult Run(CSharpCompilation compilation)
    {
        CSharpParseOptions parsing = (CSharpParseOptions)compilation.SyntaxTrees[0].Options;
        GeneratorDriverRunResult result = Generate(compilation, parsing, out Compilation output).GetRunResult();

        Dictionary<string, string> generated = result.Results
            .SelectMany(static generator => generator.GeneratedSources)
            .ToDictionary(static file => file.HintName, static file => file.SourceText.ToString(), StringComparer.Ordinal);

        return new GeneratorResult(generated, [.. DiagnosticText.InSourceOrder(result.Diagnostics)], [.. output.GetDiagnostics()]);
    }

    internal static Task VerifyAsync(ReadOnlySpan<string> sources, [CallerFilePath] string sourceFile = "")
    {
        GeneratorDriver driver = Generate(TestCompilation.Create(sources), Parsing.Preview, out Compilation output);
        string errors = DiagnosticText.Join(output.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

        if (errors.Length > 0)
            throw new InvalidOperationException($"The generated code does not compile:{Environment.NewLine}{errors}");

        return Verify(driver, sourceFile: sourceFile);
    }

    private static GeneratorDriver Generate(CSharpCompilation compilation, CSharpParseOptions parsing, out Compilation output)
    {
        GeneratorDriver driver = CSharpGeneratorDriver
            .Create([new TGenerator().AsSourceGenerator()], parseOptions: parsing)
            .RunGeneratorsAndUpdateCompilation(compilation, out output, out _);

        Exception? exception = driver.GetRunResult().Results
            .Select(static generator => generator.Exception)
            .FirstOrDefault(static exception => exception is not null);

        if (exception is not null)
            throw new InvalidOperationException($"{typeof(TGenerator).Name} threw during generation.", exception);

        return driver;
    }
}
