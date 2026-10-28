// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InteropLibraryAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptors.InteropLibraryWrongArea, Descriptors.InteropLibraryUnknown];

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

        if (InteropAreas.Resolve(method) is not InteropArea area)
            return;

        if (LibraryImports.Find(method, libraryImport) is not { ConstructorArguments: [{ Value: string library }] } attribute)
            return;

        InteropArea? owner = InteropAreas.OwnerOf(library);

        if (owner == area)
            return;

        Location location = LocationOfLibrary(attribute, context.CancellationToken) ?? method.Locations[0];

        Diagnostic diagnostic = owner is InteropArea other
            ? Diagnostic.Create(Descriptors.InteropLibraryWrongArea, location, library, area, other)
            : Diagnostic.Create(Descriptors.InteropLibraryUnknown, location, library, area);

        context.ReportDiagnostic(diagnostic);
    }

    private static Location? LocationOfLibrary(AttributeData attribute, CancellationToken cancellationToken)
    {
        if (attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken) is AttributeSyntax { ArgumentList.Arguments: [AttributeArgumentSyntax library, ..] })
            return library.GetLocation();

        return null;
    }
}
