// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InteropVisibilityAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptors.InteropVisibility];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
    }

    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;

        if (type.IsExtension || !Visibility.LeavesTheAssembly(type) || !InteropAreas.IsInInterop(type))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropVisibility, type.Locations[0], type.Name));
    }
}
