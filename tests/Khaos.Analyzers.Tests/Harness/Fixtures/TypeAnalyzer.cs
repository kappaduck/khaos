// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Analyzers.Tests.Harness.Fixtures;

internal sealed class TypeAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor TypeFound = new("TEST001", "type found", "Type '{0}' found", "Test", DiagnosticSeverity.Warning, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [TypeFound];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSymbolAction(static context => context.ReportDiagnostic(Diagnostic.Create(TypeFound, context.Symbol.Locations[0], context.Symbol.Name)), SymbolKind.NamedType);
    }
}
