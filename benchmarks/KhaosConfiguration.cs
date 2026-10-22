// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Validators;
using System.Reflection;

namespace KappaDuck.Khaos.Benchmarks;

internal sealed class KhaosConfiguration : ManualConfig
{
    private const string ArtifactsKey = "ArtifactsPath";

    public KhaosConfiguration()
    {
        AddColumnProvider(DefaultColumnProviders.Instance);
        AddLogger(ConsoleLogger.Default);
        AddExporter(MarkdownExporter.GitHub);

        AddExporter(JsonExporter.FullCompressed);
        AddDiagnoser(MemoryDiagnoser.Default);

        AddValidator(JitOptimizationsValidator.FailOnError);

        WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest));
        WithArtifactsPath(ResolveArtifactsPath());
    }

    /// <summary>
    /// Reports go next to the benchmark project rather than next to wherever the run was started
    /// from. The path is baked in by the project file, because the host runs out of the shared
    /// output folder and cannot work out where its own sources live.
    /// </summary>
    private static string ResolveArtifactsPath()
    {
        string? path = typeof(KhaosConfiguration).Assembly
                                            .GetCustomAttributes<AssemblyMetadataAttribute>()
                                            .FirstOrDefault(attribute => attribute.Key == ArtifactsKey)?.Value;

        return string.IsNullOrEmpty(path) ? Path.Combine(Directory.GetCurrentDirectory(), "benchmarks") : path;
    }
}
