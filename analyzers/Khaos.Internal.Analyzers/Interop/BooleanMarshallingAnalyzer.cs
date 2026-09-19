// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BooleanMarshallingAnalyzer : DiagnosticAnalyzer
{
    private const string LibraryImport = "System.Runtime.InteropServices.LibraryImportAttribute";
    private const string MarshalAs = "System.Runtime.InteropServices.MarshalAsAttribute";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptors.BooleanMarshalling, Descriptors.BooleanField];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(static start =>
        {
            ImmutableArray<INamedTypeSymbol> libraryImport = start.Compilation.GetTypesByMetadataName(LibraryImport);
            ImmutableArray<INamedTypeSymbol> marshalAs = start.Compilation.GetTypesByMetadataName(MarshalAs);

            start.RegisterSyntaxNodeAction(syntax => AnalyzeMethod(syntax, libraryImport, marshalAs), SyntaxKind.MethodDeclaration);
            start.RegisterSyntaxNodeAction(AnalyzeField, SyntaxKind.FieldDeclaration);
        });
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context, ImmutableArray<INamedTypeSymbol> libraryImport, ImmutableArray<INamedTypeSymbol> marshalAs)
    {
        MethodDeclarationSyntax declaration = (MethodDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken) is not IMethodSymbol method)
            return;

        if (method.PartialDefinitionPart is not null)
            return;

        if (!HasAttribute(method.GetAttributes(), libraryImport))
            return;

        if (InteropAreas.Resolve(method) is not InteropArea area)
            return;

        if (method.ReturnType.SpecialType is SpecialType.System_Boolean && Marshalling(method.GetReturnTypeAttributes(), marshalAs) != area.Convention())
            Report(context, declaration.ReturnType.GetLocation(), $"The return value of '{method.Name}'", area, "return: ");

        foreach (IParameterSymbol parameter in method.Parameters)
        {
            if (parameter.Type.SpecialType is not SpecialType.System_Boolean)
                continue;

            if (Marshalling(parameter.GetAttributes(), marshalAs) == area.Convention())
                continue;

            ParameterSyntax syntax = declaration.ParameterList.Parameters[parameter.Ordinal];

            Report(context, (syntax.Type ?? (SyntaxNode)syntax).GetLocation(), $"Parameter '{parameter.Name}' of '{method.Name}'", area, string.Empty);
        }
    }

    private static void AnalyzeField(SyntaxNodeAnalysisContext context)
    {
        FieldDeclarationSyntax declaration = (FieldDeclarationSyntax)context.Node;

        foreach (VariableDeclaratorSyntax variable in declaration.Declaration.Variables)
        {
            if (context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken) is not IFieldSymbol field)
                continue;

            if (field.IsStatic || field.IsConst || field.Type.SpecialType is not SpecialType.System_Boolean || field.ContainingType.TypeKind is not TypeKind.Struct)
                continue;

            if (InteropAreas.Resolve(field) is not InteropArea area)
                continue;

            context.ReportDiagnostic(Diagnostic.Create(Descriptors.BooleanField, declaration.Declaration.Type.GetLocation(), $"Field '{field.ContainingType.Name}.{field.Name}'", area.Name, area.FieldType));
        }
    }

    private static void Report(SyntaxNodeAnalysisContext context, Location location, string subject, InteropArea area, string target)
        => context.ReportDiagnostic(Diagnostic.Create(Descriptors.BooleanMarshalling, location, subject, area.Name, $"{target}MarshalAs(UnmanagedType.{area.ConventionName()})"));

    private static bool HasAttribute(ImmutableArray<AttributeData> attributes, ImmutableArray<INamedTypeSymbol> candidates)
        => attributes.Any(attribute => attribute.AttributeClass is not null && candidates.Contains(attribute.AttributeClass, SymbolEqualityComparer.Default));

    private static UnmanagedType? Marshalling(ImmutableArray<AttributeData> attributes, ImmutableArray<INamedTypeSymbol> marshalAs)
    {
        foreach (AttributeData attribute in attributes)
        {
            if (attribute.AttributeClass is null || !marshalAs.Contains(attribute.AttributeClass, SymbolEqualityComparer.Default))
                continue;

            return attribute.ConstructorArguments switch
            {
                [{ Value: int value }, ..] => (UnmanagedType)value,
                [{ Value: short value }, ..] => (UnmanagedType)value,
                _ => null
            };
        }

        return null;
    }
}
