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

    private static string ResolveArtifactsPath()
    {
        return typeof(KhaosConfiguration).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                                                  .Single(attribute => attribute.Key == "ArtifactsPath").Value!;
    }
}
