// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InteropBoolMarshallingAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptors.InteropBoolReturn, Descriptors.InteropBoolParameter];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(static start =>
        {
            if (LibraryImports.AttributeType(start.Compilation) is not { } libraryImport)
                return;

            if (start.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.MarshalAsAttribute") is not { } marshalAs)
                return;

            start.RegisterSymbolAction(context => AnalyzeMethod(context, libraryImport, marshalAs), SymbolKind.Method);
        });
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context, INamedTypeSymbol libraryImport, INamedTypeSymbol marshalAs)
    {
        IMethodSymbol method = (IMethodSymbol)context.Symbol;

        if (InteropAreas.Resolve(method) is not InteropArea area)
            return;

        if (LibraryImports.Find(method, libraryImport) is not { } attribute)
            return;

        if (LibraryImports.DeclarationOf(attribute, context.CancellationToken) is not { } declaration)
            return;

        UnmanagedType expected = InteropAreas.BoolConvention(area);

        if (IsBool(method.ReturnType) && !Follows(method.GetReturnTypeAttributes(), marshalAs, expected))
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropBoolReturn, declaration.ReturnType.GetLocation(), method.Name, area, expected));

        foreach (IParameterSymbol parameter in method.Parameters)
        {
            if (!IsBool(parameter.Type) || Follows(parameter.GetAttributes(), marshalAs, expected))
                continue;

            Location location = declaration.ParameterList.Parameters[parameter.Ordinal].Type?.GetLocation() ?? parameter.Locations[0];
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropBoolParameter, location, parameter.Name, method.Name, area, expected));
        }
    }

    private static bool IsBool(ITypeSymbol type) => type.SpecialType == SpecialType.System_Boolean;

    private static bool Follows(ImmutableArray<AttributeData> attributes, INamedTypeSymbol marshalAs, UnmanagedType expected)
    {
        AttributeData? marshalling = attributes.FirstOrDefault(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, marshalAs));
        return marshalling is { ConstructorArguments: [{ Value: var value }] } && KindOf(value) == expected;
    }

    private static UnmanagedType? KindOf(object? value) => value switch
    {
        int kind => (UnmanagedType)kind,
        short kind => (UnmanagedType)kind,
        _ => null
    };
}
