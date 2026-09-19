// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PublicPointerAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptors.PublicPointer];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSymbolAction(AnalyzeMember, SymbolKind.Method, SymbolKind.Property, SymbolKind.Field);
    }

    private static void AnalyzeMember(SymbolAnalysisContext context)
    {
        ISymbol symbol = context.Symbol;

        if (symbol.IsImplicitlyDeclared || symbol is IMethodSymbol { AssociatedSymbol: not null })
            return;

        if (!Visibility.LeavesTheAssembly(symbol))
            return;

        if (!Exposes(symbol))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Descriptors.PublicPointer, symbol.Locations[0], $"{symbol.ContainingType.Name}.{symbol.Name}"));
    }

    private static bool Exposes(ISymbol symbol) => symbol switch
    {
        IMethodSymbol method => IsPointer(method.ReturnType) || method.Parameters.Any(parameter => IsPointer(parameter.Type)),
        IPropertySymbol property => IsPointer(property.Type) || property.Parameters.Any(parameter => IsPointer(parameter.Type)),
        IFieldSymbol field => IsPointer(field.Type),
        _ => false
    };

    private static bool IsPointer(ITypeSymbol type)
        => type.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer
        || (type is IArrayTypeSymbol array && IsPointer(array.ElementType));
}
