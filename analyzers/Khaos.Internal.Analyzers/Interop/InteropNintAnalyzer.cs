// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InteropNintAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        [Descriptors.InteropNintReturn, Descriptors.InteropNintParameter, Descriptors.InteropNintField, Descriptors.InteropNintProperty];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(static start =>
        {
            INamedTypeSymbol? inlineArray = start.Compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.InlineArrayAttribute");
            start.RegisterSymbolAction(context => AnalyzeType(context, inlineArray), SymbolKind.NamedType);
        });
    }

    private static void AnalyzeType(SymbolAnalysisContext context, INamedTypeSymbol? inlineArray)
    {
        INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;

        if (InteropAreas.Resolve(type) is null || IsNintWrapper(type, inlineArray))
            return;

        foreach (ISymbol member in type.GetMembers())
        {
            if (member is IMethodSymbol method && HasOwnSignature(method))
                AnalyzeMethod(context, method);
            else if (member is IFieldSymbol { IsConst: false, AssociatedSymbol: null } field && UsesNint(field.Type))
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropNintField, TypeLocationOf(field, context.CancellationToken), type.Name, field.Name));
            else if (member is IPropertySymbol property && UsesNint(property.Type))
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropNintProperty, TypeLocationOf(property, context.CancellationToken), type.Name, property.Name));
        }
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context, IMethodSymbol method)
    {
        SyntaxNode? declaration = method.DeclaringSyntaxReferences is [var reference, ..] ? reference.GetSyntax(context.CancellationToken) : null;
        string name = method.MethodKind == MethodKind.Constructor ? method.ContainingType.Name : method.Name;

        if (UsesNint(method.ReturnType))
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropNintReturn, ReturnTypeLocationOf(declaration) ?? method.Locations[0], name));

        foreach (IParameterSymbol parameter in method.Parameters)
        {
            if (!UsesNint(parameter.Type))
                continue;

            Location location = ParameterTypeLocationOf(declaration, parameter.Ordinal) ?? parameter.Locations[0];
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropNintParameter, location, parameter.Name, name));
        }
    }

    private static bool IsNintWrapper(INamedTypeSymbol type, INamedTypeSymbol? inlineArray)
    {
        if (type.TypeKind != TypeKind.Struct || IsInlineArray(type, inlineArray))
            return false;

        IFieldSymbol? wrapped = null;

        foreach (ISymbol member in type.GetMembers())
        {
            if (member is not IFieldSymbol { IsStatic: false } field)
                continue;

            if (wrapped is not null)
                return false;

            wrapped = field;
        }

        return wrapped?.Type.SpecialType == SpecialType.System_IntPtr;
    }

    private static bool IsInlineArray(INamedTypeSymbol type, INamedTypeSymbol? inlineArray)
        => inlineArray is not null && type.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, inlineArray));

    private static bool HasOwnSignature(IMethodSymbol method) => method is
    {
        IsImplicitlyDeclared: false,
        PartialDefinitionPart: null,
        MethodKind: MethodKind.Ordinary or MethodKind.Constructor or MethodKind.UserDefinedOperator or MethodKind.Conversion
    };

    private static bool UsesNint(ITypeSymbol type) => type switch
    {
        { SpecialType: SpecialType.System_IntPtr } => true,
        IPointerTypeSymbol pointer => UsesNint(pointer.PointedAtType),
        IFunctionPointerTypeSymbol function => UsesNint(function.Signature.ReturnType) || function.Signature.Parameters.Any(static parameter => UsesNint(parameter.Type)),
        _ => false
    };

    private static Location? ReturnTypeLocationOf(SyntaxNode? declaration) => declaration switch
    {
        MethodDeclarationSyntax method => method.ReturnType.GetLocation(),
        OperatorDeclarationSyntax @operator => @operator.ReturnType.GetLocation(),
        ConversionOperatorDeclarationSyntax conversion => conversion.Type.GetLocation(),
        _ => null
    };

    private static Location? ParameterTypeLocationOf(SyntaxNode? declaration, int ordinal)
    {
        ParameterListSyntax? parameters = declaration switch
        {
            BaseMethodDeclarationSyntax method => method.ParameterList,
            TypeDeclarationSyntax primary => primary.ParameterList,
            _ => null
        };

        return parameters?.Parameters[ordinal].Type?.GetLocation();
    }

    private static Location TypeLocationOf(IPropertySymbol property, CancellationToken cancellationToken)
    {
        if (property.DeclaringSyntaxReferences is [var reference, ..] && reference.GetSyntax(cancellationToken) is BasePropertyDeclarationSyntax declaration)
            return declaration.Type.GetLocation();

        return property.Locations[0];
    }

    private static Location TypeLocationOf(IFieldSymbol field, CancellationToken cancellationToken)
    {
        if (field.DeclaringSyntaxReferences is [var reference, ..] && reference.GetSyntax(cancellationToken).Parent is VariableDeclarationSyntax declaration)
            return declaration.Type.GetLocation();

        return field.Locations[0];
    }
}
