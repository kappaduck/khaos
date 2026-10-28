// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InteropEntryPointAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptors.InteropEntryPointMissing, Descriptors.InteropEntryPointEmpty];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(static start =>
        {
            if (LibraryImports.AttributeType(start.Compilation) is not { } libraryImport)
                return;

            start.RegisterSymbolAction(context => AnalyzeMethod(context, libraryImport), SymbolKind.Method);
        });
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context, INamedTypeSymbol libraryImport)
    {
        IMethodSymbol method = (IMethodSymbol)context.Symbol;

        if (InteropAreas.Resolve(method) is null)
            return;

        if (LibraryImports.Find(method, libraryImport) is not { } attribute)
            return;

        Location attributeLocation = attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation() ?? method.Locations[0];

        if (LibraryImports.EntryPointOf(attribute) is not { } entryPoint)
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropEntryPointMissing, attributeLocation, method.Name));
            return;
        }

        if (!string.IsNullOrWhiteSpace(entryPoint.Value as string))
            return;

        Location location = LibraryImports.EntryPointArgumentOf(attribute, context.CancellationToken)?.GetLocation() ?? attributeLocation;
        context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropEntryPointEmpty, location, method.Name));
    }
}
