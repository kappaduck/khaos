// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InteropEntryPointAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        Descriptors.InteropEntryPointMissing,
        Descriptors.InteropEntryPointEmpty
    ];

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

        if (!InteropAreas.TryResolve(method, out _))
            return;

        if (!LibraryImports.TryGetAttributeData(method, libraryImport, out AttributeData? attribute))
            return;

        Location attributeLocation = attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation() ?? method.Locations[0];

        if (!LibraryImports.TryGetEntryPoint(attribute, out TypedConstant entryPoint))
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropEntryPointMissing, attributeLocation, method.Name));
            return;
        }

        if (!string.IsNullOrWhiteSpace(entryPoint.Value as string))
            return;

        Location location = LibraryImports.TryGetEntryPointArgument(attribute, context.CancellationToken, out AttributeArgumentSyntax? argument) ? argument.GetLocation() : attributeLocation;
        context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropEntryPointEmpty, location, method.Name));
    }
}
