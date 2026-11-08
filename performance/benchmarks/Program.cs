// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using System.Reflection;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                 .Run(args, new Configuration());

file sealed class Configuration : ManualConfig
{
    public Configuration()
    {
        AddColumnProvider(DefaultColumnProviders.Instance);
        AddLogger(ConsoleLogger.Default);
        AddExporter(MarkdownExporter.GitHub, JsonExporter.FullCompressed);

        AddDiagnoser(MemoryDiagnoser.Default);
        AddValidator(JitOptimizationsValidator.FailOnError);

        WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest));
        WithArtifactsPath(ResolveArtifactsPath());

        static string ResolveArtifactsPath()
        {
            Assembly assembly = typeof(Configuration).Assembly;
            return assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                           .Single(a => a.Key == "ArtifactsPath").Value!;
        }
    }
}
