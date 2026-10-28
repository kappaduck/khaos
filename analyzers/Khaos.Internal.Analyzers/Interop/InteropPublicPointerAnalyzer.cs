// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InteropPublicPointerAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptors.InteropPublicPointer, Descriptors.InteropPublicNint];

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

        if (type is { TypeKind: TypeKind.Delegate, DelegateInvokeMethod: { } invoke })
        {
            if (Visibility.LeavesTheAssembly(type))
                Report(context, type, type.Name, ExposureOf(invoke));

            return;
        }

        foreach (ISymbol member in type.GetMembers())
        {
            if (!Visibility.LeavesTheAssembly(member))
                continue;

            Exposure exposure = member switch
            {
                IMethodSymbol method when Signatures.HasOwn(method) => ExposureOf(method),
                IPropertySymbol property => Worst(ExposureOf(property.Type), ExposureOf(property.Parameters)),
                IFieldSymbol { IsConst: false, AssociatedSymbol: null } field => ExposureOf(field.Type),
                _ => Exposure.None
            };

            string name = member is IMethodSymbol { MethodKind: MethodKind.Constructor } ? type.Name : $"{type.Name}.{member.Name}";
            Report(context, member, name, exposure);
        }
    }

    private static void Report(SymbolAnalysisContext context, ISymbol symbol, string name, Exposure exposure)
    {
        if (exposure == Exposure.None)
            return;

        DiagnosticDescriptor descriptor = exposure == Exposure.Pointer ? Descriptors.InteropPublicPointer : Descriptors.InteropPublicNint;
        context.ReportDiagnostic(Diagnostic.Create(descriptor, symbol.Locations[0], name));
    }

    private static Exposure ExposureOf(IMethodSymbol signature)
        => Worst(ExposureOf(signature.ReturnType), ExposureOf(signature.Parameters));

    private static Exposure ExposureOf(ImmutableArray<IParameterSymbol> parameters)
    {
        Exposure worst = Exposure.None;

        foreach (IParameterSymbol parameter in parameters)
            worst = Worst(worst, ExposureOf(parameter.Type));

        return worst;
    }

    private static Exposure ExposureOf(ITypeSymbol type)
    {
        switch (type)
        {
            case IPointerTypeSymbol or IFunctionPointerTypeSymbol:
                return Exposure.Pointer;

            case { SpecialType: SpecialType.System_IntPtr }:
                return Exposure.Nint;

            case IArrayTypeSymbol array:
                return ExposureOf(array.ElementType);

            case INamedTypeSymbol { IsGenericType: true } generic:
                Exposure worst = Exposure.None;

                foreach (ITypeSymbol argument in generic.TypeArguments)
                    worst = Worst(worst, ExposureOf(argument));

                return worst;

            default:
                return Exposure.None;
        }
    }

    private static Exposure Worst(Exposure first, Exposure second) => first > second ? first : second;

    private enum Exposure
    {
        None,
        Nint,
        Pointer
    }
}
