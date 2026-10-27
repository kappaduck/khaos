// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace KappaDuck.Khaos.Testing.Harness;

internal static class IncrementalChecks
{
    private static readonly Type[] _rootingTypes =
    [
        typeof(Compilation),
            typeof(IOperation),
            typeof(ISymbol),
            typeof(Location),
            typeof(SemanticModel),
            typeof(SyntaxNode),
            typeof(SyntaxTree)
    ];

    internal static string Rerun<TGenerator>(ReadOnlySpan<string> sources, params ReadOnlySpan<string> trackingNames) where TGenerator : IIncrementalGenerator, new()
    {
        CSharpCompilation compilation = TestCompilation.Create(sources);
        List<string> failures = [];

        (GeneratorDriverRunResult first, GeneratorDriverRunResult second) = RunTwice<TGenerator>(compilation, compilation.Clone(), failures);
        GeneratorDriverRunResult fromScratch = Create<TGenerator>().RunGenerators(TestCompilation.Create(sources)).GetRunResult();

        if (!string.Equals(Sources(first), Sources(fromScratch), StringComparison.Ordinal))
            failures.Add("Two runs from scratch produced different sources. The generator is not deterministic.");

        Check(first, second, trackingNames, [], failures);
        AddRegeneratedOutputs(second, failures);

        return string.Join(Environment.NewLine, failures);
    }

    internal static string Edit<TGenerator>(ReadOnlySpan<string> sources, ReadOnlySpan<string> edited, ReadOnlySpan<string> reused, ReadOnlySpan<string> recomputed) where TGenerator : IIncrementalGenerator, new()
    {
        CSharpCompilation compilation = TestCompilation.Create(sources);
        List<string> failures = [];

        (GeneratorDriverRunResult first, GeneratorDriverRunResult second) = RunTwice<TGenerator>(compilation, TestCompilation.Edit(compilation, edited), failures);
        Check(first, second, reused, recomputed, failures);

        return string.Join(Environment.NewLine, failures);
    }

    private static (GeneratorDriverRunResult First, GeneratorDriverRunResult Second) RunTwice<TGenerator>(CSharpCompilation compilation, CSharpCompilation next, List<string> failures) where TGenerator : IIncrementalGenerator, new()
    {
        GeneratorDriver driver = Create<TGenerator>().RunGenerators(compilation);
        GeneratorDriverRunResult first = driver.GetRunResult();
        GeneratorDriverRunResult second = driver.RunGenerators(next).GetRunResult();

        AddExceptions(first, "first", failures);
        AddExceptions(second, "second", failures);

        return (first, second);
    }

    private static CSharpGeneratorDriver Create<TGenerator>() where TGenerator : IIncrementalGenerator, new()
    {
        GeneratorDriverOptions options = new(IncrementalGeneratorOutputKind.None, true);
        return CSharpGeneratorDriver.Create([new TGenerator().AsSourceGenerator()], null, Parsing.Preview, null, options);
    }

    private static void AddExceptions(GeneratorDriverRunResult result, string run, List<string> failures)
    {
        foreach (GeneratorRunResult generator in result.Results.Where(static generator => generator.Exception is not null))
            failures.Add($"The generator threw on the {run} run: {generator.Exception}");
    }

    private static string Sources(GeneratorDriverRunResult result)
        => string.Join(Environment.NewLine, result.GeneratedTrees.Select(static tree => tree.ToString()));

    private static void Check(GeneratorDriverRunResult first, GeneratorDriverRunResult second, ReadOnlySpan<string> reused, ReadOnlySpan<string> recomputed, List<string> failures)
    {
        foreach (string trackingName in reused)
        {
            AddRootedObjects(first, trackingName, failures);
            AddSteps(second, trackingName, reuse: true, failures);
        }

        foreach (string trackingName in recomputed)
        {
            AddRootedObjects(first, trackingName, failures);
            AddSteps(second, trackingName, reuse: false, failures);
        }
    }

    private static void AddRootedObjects(GeneratorDriverRunResult result, string trackingName, List<string> failures)
    {
        if (!TryGetSteps(result, trackingName, out ImmutableArray<IncrementalGeneratorRunStep> steps))
            return;

        HashSet<object> visited = [with(ReferenceEqualityComparer.Instance)];

        foreach (IncrementalGeneratorRunStep step in steps)
        {
            foreach ((object value, _) in step.Outputs)
                Visit(value, trackingName, string.Empty, visited, failures);
        }
    }

    private static void AddSteps(GeneratorDriverRunResult result, string trackingName, bool reuse, List<string> failures)
    {
        if (!TryGetSteps(result, trackingName, out ImmutableArray<IncrementalGeneratorRunStep> steps))
        {
            failures.Add($"No tracked step named '{trackingName}' produced anything. Add .WithTrackingName(\"{trackingName}\") to the provider and make sure the sources trigger it.");
            return;
        }

        List<IncrementalStepRunReason> reasons = [.. steps.SelectMany(static step => step.Outputs).Select(static output => output.Reason)];

        if (reasons.Count == 0)
        {
            failures.Add($"'{trackingName}' produced no outputs, so the expectation cannot be checked.");
            return;
        }

        bool recomputed = reasons.Exists(static reason => reason is not (IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged));

        if (reuse && recomputed)
            failures.Add($"'{trackingName}' was recomputed on the second run ({string.Join(", ", reasons)}) but was expected to be reused. Its model does not compare by value.");

        if (!reuse && !recomputed)
            failures.Add($"'{trackingName}' was reused on the second run but was expected to be recomputed. The edit never reached this step, so the test proves nothing.");
    }

    private static bool TryGetSteps(GeneratorDriverRunResult result, string trackingName, out ImmutableArray<IncrementalGeneratorRunStep> steps)
    {
        foreach (GeneratorRunResult generator in result.Results)
        {
            if (generator.TrackedSteps.TryGetValue(trackingName, out steps) && !steps.IsDefaultOrEmpty)
                return true;
        }

        steps = [];
        return false;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Test-only reflection over generator models.")]
    private static void Visit(object? value, string trackingName, string path, HashSet<object> visited, List<string> failures)
    {
        if (value is null or string)
            return;

        Type type = value.GetType();

        if (type.IsPrimitive || type.IsEnum)
            return;

        if (!visited.Add(value))
            return;

        Type? rooting = Array.Find(_rootingTypes, rooting => rooting.IsAssignableFrom(type));

        if (rooting is not null)
        {
            failures.Add($"'{trackingName}' holds a {rooting.Name} at '{trackingName}{path}'. It roots the compilation and never compares by value.");
            return;
        }

        if (value is IEnumerable sequence)
        {
            if (IsDefaultImmutableArray(type, value))
                return;

            int index = 0;

            foreach (object? item in sequence)
                Visit(item, trackingName, $"{path}[{index++}]", visited, failures);

            return;
        }

#pragma warning disable S3011 // The walker must read record backing fields to prove a model does not root the compilation.
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
#pragma warning restore S3011

        foreach (FieldInfo field in fields)
            Visit(field.GetValue(value), trackingName, $"{path}.{field.Name}", visited, failures);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Test-only reflection over generator models.")]
    private static bool IsDefaultImmutableArray(Type type, object value)
    {
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(ImmutableArray<>))
            return false;

        return type.GetProperty(nameof(ImmutableArray<>.IsDefault))?.GetValue(value) is true;
    }

    private static void AddRegeneratedOutputs(GeneratorDriverRunResult result, List<string> failures)
    {
        IncrementalStepRunReason[] reasons =
        [
            .. result.Results
                .SelectMany(static generator => generator.TrackedOutputSteps.Values)
                .SelectMany(static steps => steps)
                .SelectMany(static step => step.Outputs)
                .Select(static output => output.Reason)
                .Where(static reason => reason != IncrementalStepRunReason.Cached)
        ];

        if (reasons.Length > 0)
            failures.Add($"The second run produced its sources again ({string.Join(", ", reasons)}) although nothing changed.");
    }
}
