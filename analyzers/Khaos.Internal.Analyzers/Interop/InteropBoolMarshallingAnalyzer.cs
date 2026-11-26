// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InteropBoolMarshallingAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        Descriptors.InteropBoolReturn,
        Descriptors.InteropBoolParameter
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(static start =>
        {
            if (!LibraryImports.TryGetAttribute(start.Compilation, out INamedTypeSymbol? libraryImport))
                return;

            if (!TryGetMarshalAsAttribute(start.Compilation, out INamedTypeSymbol? marshalAs))
                return;

            start.RegisterSymbolAction(ctx => AnalyzeMethod(ctx, libraryImport, marshalAs), SymbolKind.Method);
        });
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context, INamedTypeSymbol libraryImport, INamedTypeSymbol marshalAs)
    {
        IMethodSymbol method = (IMethodSymbol)context.Symbol;

        if (!InteropAreas.TryResolve(method, out InteropArea area))
            return;

        if (!LibraryImports.TryGetAttributeData(method, libraryImport, out _))
            return;

        UnmanagedType expected = InteropAreas.GetBoolConvention(area);

        if (IsBool(method.ReturnType) && !IsMarshalledAs(method.GetReturnTypeAttributes(), marshalAs, expected))
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropBoolReturn, TypeLocations.OfReturn(method, context.CancellationToken), method.Name, area, expected));

        foreach (IParameterSymbol parameter in method.Parameters)
        {
            if (!IsBool(parameter.Type) || IsMarshalledAs(parameter.GetAttributes(), marshalAs, expected))
                continue;

            Location location = TypeLocations.Of(parameter, context.CancellationToken);
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropBoolParameter, location, parameter.Name, method.Name, area, expected));
        }
    }

    private static bool TryGetMarshalAsAttribute(Compilation compilation, [NotNullWhen(true)] out INamedTypeSymbol? attribute)
    {
        attribute = compilation.GetTypeByMetadataName("System.Runtime.InteropServices.MarshalAsAttribute");
        return attribute is not null;
    }

    private static bool IsBool(ITypeSymbol type) => type.SpecialType == SpecialType.System_Boolean;

    private static bool IsMarshalledAs(ImmutableArray<AttributeData> attributes, INamedTypeSymbol marshalAs, UnmanagedType expected)
    {
        AttributeData? marshalling = attributes.FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, marshalAs));
        return marshalling is { ConstructorArguments: [{ Value: var value }] } && HasSameUnmanagedType(value, expected);

        static bool HasSameUnmanagedType(object? value, UnmanagedType expected)
        {
            return expected == value switch
            {
                int kind => (UnmanagedType)kind,
                short kind => (UnmanagedType)kind,
                _ => null
            };
        }
    }
}
