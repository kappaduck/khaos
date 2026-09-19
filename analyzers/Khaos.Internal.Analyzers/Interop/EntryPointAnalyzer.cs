// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EntryPointAnalyzer : DiagnosticAnalyzer
{
    private const string EntryPoint = "EntryPoint";
    private const string LibraryImport = "System.Runtime.InteropServices.LibraryImportAttribute";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptors.EntryPoint];

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

        if (InteropAreas.Resolve(method) is null)
            return;

        if (Attributes.Find(method.GetAttributes(), libraryImport) is not AttributeData attribute)
            return;

        if (Names(attribute))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Descriptors.EntryPoint, Attributes.Location(attribute, declaration, context.CancellationToken), method.Name));
    }

    private static bool Names(AttributeData attribute)
    {
        foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
        {
            if (argument.Key == EntryPoint)
                return argument.Value.Value is string entryPoint && !string.IsNullOrWhiteSpace(entryPoint);
        }

        return false;
    }
}
