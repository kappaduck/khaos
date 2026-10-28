// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Analyzers.Tests.Harness.Fixtures;

internal sealed class ThrowingAnalyzer : DiagnosticAnalyzer
{
    internal const string Message = "The analyzer fixture failed on purpose.";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [TypeAnalyzer.TypeFound];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSymbolAction(static _ => throw new InvalidOperationException(Message), SymbolKind.NamedType);
    }
}
