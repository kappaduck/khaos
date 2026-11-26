// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InteropLibraryAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        Descriptors.InteropLibraryWrongArea,
        Descriptors.InteropLibraryUnknown,
        Descriptors.InteropLibraryWrongEntryPoint
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

        if (!InteropAreas.TryResolve(method, out InteropArea area))
            return;

        if (!LibraryImports.TryGetAttributeData(method, libraryImport, out AttributeData? attribute) || attribute is not { ConstructorArguments: [{ Value: string library }] })
            return;

        bool owned = InteropAreas.TryGetOwner(library, out InteropArea owner);

        if (owned && owner == area)
        {
            AnalyzeEntryPoint(context, method, attribute, library);
            return;
        }

        Location location = LocationOfLibrary(attribute, context.CancellationToken) ?? method.Locations[0];

        Diagnostic diagnostic = owned
            ? Diagnostic.Create(Descriptors.InteropLibraryWrongArea, location, library, area, owner)
            : Diagnostic.Create(Descriptors.InteropLibraryUnknown, location, library, area);

        context.ReportDiagnostic(diagnostic);
    }

    private static void AnalyzeEntryPoint(SymbolAnalysisContext context, IMethodSymbol method, AttributeData attribute, string library)
    {
        if (!InteropAreas.TryGetPrefix(library, out string? prefix))
            return;

        if (!LibraryImports.TryGetEntryPoint(attribute, out TypedConstant entryPointArgument) || entryPointArgument.Value is not string entryPoint || string.IsNullOrWhiteSpace(entryPoint))
            return;

        if (entryPoint.StartsWith(prefix, StringComparison.Ordinal))
            return;

        Location location = LibraryImports.TryGetEntryPointArgument(attribute, context.CancellationToken, out AttributeArgumentSyntax? argument) ? argument.GetLocation() : method.Locations[0];
        context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropLibraryWrongEntryPoint, location, entryPoint, library, prefix));
    }

    private static Location? LocationOfLibrary(AttributeData attribute, CancellationToken cancellationToken)
    {
        if (attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken) is AttributeSyntax { ArgumentList.Arguments: [AttributeArgumentSyntax library, ..] })
            return library.GetLocation();

        return null;
    }
}
