// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
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
            if (member is IMethodSymbol method && Signatures.HasOwn(method))
                AnalyzeMethod(context, method);
            else if (member is IFieldSymbol { IsConst: false, AssociatedSymbol: null } field && UsesNint(field.Type))
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropNintField, TypeLocations.Of(field, context.CancellationToken), type.Name, field.Name));
            else if (member is IPropertySymbol property && UsesNint(property.Type))
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropNintProperty, TypeLocations.Of(property, context.CancellationToken), type.Name, property.Name));
        }
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context, IMethodSymbol method)
    {
        string name = Signatures.NameOf(method);

        if (UsesNint(method.ReturnType))
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropNintReturn, TypeLocations.OfReturn(method, context.CancellationToken), name));

        foreach (IParameterSymbol parameter in method.Parameters.Where(static parameter => UsesNint(parameter.Type)))
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropNintParameter, TypeLocations.Of(parameter, context.CancellationToken), parameter.Name, name));
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

    private static bool UsesNint(ITypeSymbol type)
        => Signatures.Contains(type, static candidate => candidate.SpecialType == SpecialType.System_IntPtr);
}
