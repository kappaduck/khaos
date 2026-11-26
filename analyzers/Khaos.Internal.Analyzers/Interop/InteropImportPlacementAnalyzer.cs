// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InteropImportPlacementAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptors.InteropImportOutsideArea];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(static start =>
        {
            if (!LibraryImports.TryGetAttribute(start.Compilation, out INamedTypeSymbol? libraryImport))
                return;

            start.RegisterSymbolAction(ctx => AnalyzeMethod(ctx, libraryImport), SymbolKind.Method);
        });
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context, INamedTypeSymbol libraryImport)
    {
        IMethodSymbol method = (IMethodSymbol)context.Symbol;

        if (InteropAreas.TryResolve(method, out _) || !LibraryImports.TryGetAttributeData(method, libraryImport, out _))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropImportOutsideArea, method.Locations[0], method.Name));
    }
}
