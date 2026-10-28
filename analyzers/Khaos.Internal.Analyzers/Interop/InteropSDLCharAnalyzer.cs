// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InteropSDLCharAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        [Descriptors.InteropSDLCharReturn, Descriptors.InteropSDLCharParameter, Descriptors.InteropSDLCharField, Descriptors.InteropSDLCharProperty];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
    }

    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;

        if (InteropAreas.Resolve(type) != InteropArea.SDL)
            return;

        foreach (ISymbol member in type.GetMembers())
        {
            if (member is IMethodSymbol method && Signatures.HasOwn(method))
                AnalyzeMethod(context, method);
            else if (member is IFieldSymbol { IsConst: false, AssociatedSymbol: null } field && UsesChar(field.Type))
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropSDLCharField, TypeLocations.Of(field, context.CancellationToken), type.Name, field.Name));
            else if (member is IPropertySymbol property && UsesChar(property.Type))
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropSDLCharProperty, TypeLocations.Of(property, context.CancellationToken), type.Name, property.Name));
        }
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context, IMethodSymbol method)
    {
        string name = Signatures.NameOf(method);

        if (UsesChar(method.ReturnType))
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropSDLCharReturn, TypeLocations.OfReturn(method, context.CancellationToken), name));

        foreach (IParameterSymbol parameter in method.Parameters.Where(static parameter => UsesChar(parameter.Type)))
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropSDLCharParameter, TypeLocations.Of(parameter, context.CancellationToken), parameter.Name, name));
    }

    private static bool UsesChar(ITypeSymbol type)
        => Signatures.Contains(type, static candidate => candidate.SpecialType == SpecialType.System_Char);
}
