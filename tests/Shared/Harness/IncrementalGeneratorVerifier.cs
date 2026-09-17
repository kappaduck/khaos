// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace KappaDuck.Khaos.Testing.Harness;

internal static class IncrementalGeneratorVerifier
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

    internal static string Verify<TGenerator>(string[] sources, params string[] trackingNames) where TGenerator : IIncrementalGenerator, new()
        => Run<TGenerator>(sources, sources, trackingNames, [], compareSources: true);

    internal static string VerifyEdit<TGenerator>(string[] sources, string[] edited, string[] reused, string[] recomputed) where TGenerator : IIncrementalGenerator, new()
        => Run<TGenerator>(sources, edited, reused, recomputed, compareSources: false);

    private static string Run<TGenerator>(string[] sources, string[] edited, string[] reused, string[] recomputed, bool compareSources) where TGenerator : IIncrementalGenerator, new()
    {
        GeneratorDriverOptions options = new(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true);

        GeneratorDriver driver = CSharpGeneratorDriver.Create([new TGenerator().AsSourceGenerator()], null, Parsing.Preview, null, options);

        driver = driver.RunGenerators(TestCompilation.Create(sources));
        GeneratorDriverRunResult first = driver.GetRunResult();

        driver = driver.RunGenerators(TestCompilation.Create(edited));
        GeneratorDriverRunResult second = driver.GetRunResult();

        List<string> failures = [];

        AddExceptions(first, "first", failures);
        AddExceptions(second, "second", failures);

        if (compareSources && !string.Equals(Sources(first), Sources(second), StringComparison.Ordinal))
            failures.Add("The two runs produced different sources. The generator is not deterministic.");

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

        return string.Join(Environment.NewLine, failures);
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

    private static void AddExceptions(GeneratorDriverRunResult result, string run, List<string> failures)
    {
        foreach (GeneratorRunResult generator in result.Results.Where(static generator => generator.Exception is not null))
            failures.Add($"The generator threw on the {run} run: {generator.Exception}");
    }

    private static void AddRootedObjects(GeneratorDriverRunResult result, string trackingName, List<string> failures)
    {
        if (!TryGetSteps(result, trackingName, out ImmutableArray<IncrementalGeneratorRunStep> steps))
            return;

        HashSet<object> visited = [with(ReferenceEqualityComparer.Instance)];

        foreach (IncrementalGeneratorRunStep step in steps)
        {
            foreach ((object value, IncrementalStepRunReason _) in step.Outputs)
                Visit(value, trackingName, string.Empty, visited, failures);
        }
    }

    private static string Sources(GeneratorDriverRunResult result)
        => string.Join(Environment.NewLine, result.GeneratedTrees.Select(static tree => tree.ToString()));

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

    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Test-only reflection over generator models.")]
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

        Type? rooting = Array.Find(_rootingTypes, type.IsAssignableFrom);

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
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Test-only reflection over generator models.")]
    private static bool IsDefaultImmutableArray(Type type, object value)
    {
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(ImmutableArray<>))
            return false;

        return type.GetProperty(nameof(ImmutableArray<>.IsDefault))?.GetValue(value) is true;
    }
}
