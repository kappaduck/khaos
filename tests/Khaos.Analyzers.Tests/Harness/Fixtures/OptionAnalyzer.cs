// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Analyzers.Tests.Harness.Fixtures;

internal sealed class OptionAnalyzer : DiagnosticAnalyzer
{
    internal const string Key = "build_property.KhaosTest";

    internal static readonly DiagnosticDescriptor OptionFound = new("TEST002", "option found", "Option is '{0}'", "Test", DiagnosticSeverity.Warning, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [OptionFound];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSyntaxTreeAction(static context =>
        {
            if (context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Tree).TryGetValue(Key, out string? value))
                context.ReportDiagnostic(Diagnostic.Create(OptionFound, Location.None, value));
        });
    }
}
