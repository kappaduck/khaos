// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Reflection.Metadata;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InteropCallbackSignatureAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptors.InteropCallbackNotBlittable];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(static start =>
        {
            INamedTypeSymbol? unmanagedCallersOnly = start.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.UnmanagedCallersOnlyAttribute");
            start.RegisterSymbolAction(context => AnalyzeType(context, unmanagedCallersOnly), SymbolKind.NamedType);
        });
    }

    private static void AnalyzeType(SymbolAnalysisContext context, INamedTypeSymbol? unmanagedCallersOnly)
    {
        INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;

        foreach (ISymbol member in type.GetMembers())
        {
            SpecialType found = member switch
            {
                IMethodSymbol method when Signatures.HasOwn(method) => InSignature(method, IsCalledByNativeCode(method, unmanagedCallersOnly)),
                IPropertySymbol property => InCallbacksOf(property.Type),
                IFieldSymbol { AssociatedSymbol: null } field => InCallbacksOf(field.Type),
                _ => SpecialType.None
            };

            if (found == SpecialType.None)
                continue;

            string name = member is IMethodSymbol constructor ? Signatures.NameOf(constructor) : member.Name;
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropCallbackNotBlittable, member.Locations[0], name, KeywordOf(found), ReplacementOf(found)));
        }
    }

    private static bool IsCalledByNativeCode(IMethodSymbol method, INamedTypeSymbol? unmanagedCallersOnly)
        => unmanagedCallersOnly is not null && method.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, unmanagedCallersOnly));

    private static SpecialType InSignature(IMethodSymbol signature, bool isCallback)
    {
        foreach (ITypeSymbol type in TypesOf(signature))
        {
            SpecialType found = isCallback && IsNotBlittable(type) ? type.SpecialType : InCallbacksOf(type);

            if (found != SpecialType.None)
                return found;
        }

        return SpecialType.None;
    }

    private static SpecialType InCallbacksOf(ITypeSymbol type) => type switch
    {
        IPointerTypeSymbol pointer => InCallbacksOf(pointer.PointedAtType),
        IFunctionPointerTypeSymbol function => InSignature(function.Signature, function.Signature.CallingConvention != SignatureCallingConvention.Default),
        _ => SpecialType.None
    };

    private static IEnumerable<ITypeSymbol> TypesOf(IMethodSymbol signature)
    {
        yield return signature.ReturnType;

        foreach (IParameterSymbol parameter in signature.Parameters)
            yield return parameter.Type;
    }

    private static bool IsNotBlittable(ITypeSymbol type) => type.SpecialType is SpecialType.System_Boolean or SpecialType.System_Char;

    private static string KeywordOf(SpecialType type) => type == SpecialType.System_Boolean ? "bool" : "char";

    private static string ReplacementOf(SpecialType type) => type == SpecialType.System_Boolean ? "byte or int" : "byte or ushort";
}
