// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UntypedPointerAnalyzer : DiagnosticAnalyzer
{
    private const string UntypedPointer = "KappaDuck.Khaos.Interop.UntypedPointerAttribute";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptors.UntypedPointer];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(static start =>
        {
            ImmutableArray<INamedTypeSymbol> untyped = start.Compilation.GetTypesByMetadataName(UntypedPointer);

            start.RegisterSyntaxNodeAction(syntax => AnalyzeMethod(syntax, untyped), SyntaxKind.MethodDeclaration);
        });
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context, ImmutableArray<INamedTypeSymbol> untyped)
    {
        MethodDeclarationSyntax declaration = (MethodDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken) is not IMethodSymbol method)
            return;

        if (method.PartialDefinitionPart is not null)
            return;

        if (InteropAreas.Resolve(method) is null)
            return;

        if (!Carries(method))
            return;

        if (Explained(method.GetAttributes(), untyped))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Descriptors.UntypedPointer, declaration.Identifier.GetLocation(), method.Name));
    }

    private static bool Carries(IMethodSymbol method)
        => IsUntyped(method.ReturnType) || method.Parameters.Any(parameter => IsUntyped(parameter.Type));

    private static bool IsUntyped(ITypeSymbol type)
    {
        while (type is IPointerTypeSymbol pointer)
            type = pointer.PointedAtType;

        return type.SpecialType is SpecialType.System_Void;
    }

    private static bool Explained(ImmutableArray<AttributeData> attributes, ImmutableArray<INamedTypeSymbol> untyped)
    {
        if (Attributes.Find(attributes, untyped) is not AttributeData attribute)
            return false;

        return attribute.ConstructorArguments is [{ Value: string reason }, ..] && reason.Trim().Length > 0;
    }
}
