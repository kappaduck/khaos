// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NativeIntegerAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptors.NativeInteger];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeField, SyntaxKind.FieldDeclaration);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        MethodDeclarationSyntax declaration = (MethodDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken) is not IMethodSymbol method)
            return;

        if (method.PartialDefinitionPart is not null)
            return;

        if (InteropAreas.Resolve(method) is not InteropArea area)
            return;

        if (method.ReturnType.SpecialType is SpecialType.System_IntPtr)
            Report(context, declaration.ReturnType.GetLocation(), $"The return value of '{method.Name}'", area);

        foreach (IParameterSymbol parameter in method.Parameters)
        {
            if (parameter.Type.SpecialType is not SpecialType.System_IntPtr)
                continue;

            ParameterSyntax syntax = declaration.ParameterList.Parameters[parameter.Ordinal];
            Report(context, (syntax.Type ?? (SyntaxNode)syntax).GetLocation(), $"Parameter '{parameter.Name}' of '{method.Name}'", area);
        }
    }

    private static void AnalyzeField(SyntaxNodeAnalysisContext context)
    {
        FieldDeclarationSyntax declaration = (FieldDeclarationSyntax)context.Node;

        foreach (VariableDeclaratorSyntax variable in declaration.Declaration.Variables)
        {
            if (context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken) is not IFieldSymbol field)
                continue;

            if (field.Type.SpecialType is not SpecialType.System_IntPtr)
                continue;

            if (InteropAreas.Resolve(field) is not InteropArea area)
                continue;

            Report(context, declaration.Declaration.Type.GetLocation(), $"Field '{field.ContainingType.Name}.{field.Name}'", area);
        }
    }

    private static void Report(SyntaxNodeAnalysisContext context, Location location, string subject, InteropArea area)
        => context.ReportDiagnostic(Diagnostic.Create(Descriptors.NativeInteger, location, subject, area.ExampleType));
}
