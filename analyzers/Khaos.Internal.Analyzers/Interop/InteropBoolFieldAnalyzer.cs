// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InteropBoolFieldAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptors.InteropBoolField, Descriptors.InteropBoolProperty];

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

        if (type.TypeKind != TypeKind.Struct || InteropAreas.Resolve(type) is not InteropArea area)
            return;

        string storage = InteropAreas.BoolStorage(area);

        foreach (ISymbol member in type.GetMembers())
        {
            if (member is not IFieldSymbol { IsStatic: false, IsConst: false } field || !StoresBool(field))
                continue;

            Diagnostic diagnostic = field.AssociatedSymbol is IPropertySymbol property
                ? Diagnostic.Create(Descriptors.InteropBoolProperty, TypeLocations.Of(property, context.CancellationToken), type.Name, property.Name, area, storage)
                : Diagnostic.Create(Descriptors.InteropBoolField, TypeLocations.Of(field, context.CancellationToken), type.Name, field.Name, area, storage);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool StoresBool(IFieldSymbol field)
    {
        ITypeSymbol stored = field is { IsFixedSizeBuffer: true, Type: IPointerTypeSymbol buffer } ? buffer.PointedAtType : field.Type;
        return stored.SpecialType == SpecialType.System_Boolean;
    }
}
