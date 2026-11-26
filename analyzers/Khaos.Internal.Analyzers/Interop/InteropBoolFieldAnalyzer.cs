// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InteropBoolFieldAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        Descriptors.InteropBoolField,
        Descriptors.InteropBoolProperty
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
    }

    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;

        if (type.TypeKind is not TypeKind.Struct || !InteropAreas.TryResolve(type, out InteropArea area))
            return;

        string storage = InteropAreas.GetBoolStorage(area);

        foreach (ISymbol member in type.GetMembers())
        {
            if (member is not IFieldSymbol { IsStatic: false } field || !StoresBool(field))
                continue;

            context.ReportDiagnostic(CreateDiagnostic(field, type, area, storage, context.CancellationToken));
        }

        static bool StoresBool(IFieldSymbol field)
        {
            ITypeSymbol stored = field is { IsFixedSizeBuffer: true, Type: IPointerTypeSymbol buffer } ? buffer.PointedAtType : field.Type;
            return stored.SpecialType == SpecialType.System_Boolean;
        }
    }

    private static Diagnostic CreateDiagnostic(IFieldSymbol field, INamedTypeSymbol type, InteropArea area, string storage, CancellationToken cancellationToken)
    {
        if (field.AssociatedSymbol is IPropertySymbol property)
            return Diagnostic.Create(Descriptors.InteropBoolProperty, TypeLocations.Of(property, cancellationToken), type.Name, property.Name, area, storage);

        if (Fields.TryGetCapturedParameter(field, out IParameterSymbol? parameter))
            return Diagnostic.Create(Descriptors.InteropBoolField, TypeLocations.Of(parameter, cancellationToken), type.Name, parameter.Name, area, storage);

        return Diagnostic.Create(Descriptors.InteropBoolField, TypeLocations.Of(field, cancellationToken), type.Name, field.Name, area, storage);
    }
}
