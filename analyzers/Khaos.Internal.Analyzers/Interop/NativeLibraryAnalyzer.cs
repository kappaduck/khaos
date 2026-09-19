// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NativeLibraryAnalyzer : DiagnosticAnalyzer
{
    private const string LibraryImport = "System.Runtime.InteropServices.LibraryImportAttribute";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptors.NativeLibrary];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(static start =>
        {
            ImmutableArray<INamedTypeSymbol> libraryImport = start.Compilation.GetTypesByMetadataName(LibraryImport);

            start.RegisterSyntaxNodeAction(syntax => AnalyzeMethod(syntax, libraryImport), SyntaxKind.MethodDeclaration);
        });
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context, ImmutableArray<INamedTypeSymbol> libraryImport)
    {
        MethodDeclarationSyntax declaration = (MethodDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken) is not IMethodSymbol method)
            return;

        if (method.PartialDefinitionPart is not null)
            return;

        if (InteropAreas.Resolve(method) is not InteropArea area)
            return;

        if (Attributes.Find(method.GetAttributes(), libraryImport) is not AttributeData attribute)
            return;

        if (attribute.ConstructorArguments is not [{ Value: string library }, ..])
            return;

        InteropArea? owner = InteropAreas.ResolveLibrary(library);

        if (owner == area)
            return;

        string reason = owner is not null ? $"it belongs to {owner.Value.Name}" : "no interop area declares it";

        context.ReportDiagnostic(Diagnostic.Create(Descriptors.NativeLibrary, LibraryLocation(attribute, declaration, context.CancellationToken), library, area.Name, reason));
    }

    private static Location LibraryLocation(AttributeData attribute, SyntaxNode fallback, CancellationToken cancellationToken)
    {
        if (attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken) is AttributeSyntax syntax && syntax.ArgumentList?.Arguments is [AttributeArgumentSyntax argument, ..])
        {
            return argument.Expression.GetLocation();
        }

        return fallback.GetLocation();
    }
}
