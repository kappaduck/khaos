// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Reflection.Metadata;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InteropCallbackSignatureAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptors.InteropCallbackNotBlittable];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(static start =>
        {
            INamedTypeSymbol? unmanagedCallersOnly = start.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.UnmanagedCallersOnlyAttribute");

            start.RegisterSymbolAction(ctx => AnalyzeType(ctx, unmanagedCallersOnly), SymbolKind.NamedType);
            start.RegisterOperationAction(ctx => AnalyzeLocalFunction(ctx, unmanagedCallersOnly), OperationKind.LocalFunction);
        });
    }

    private static void AnalyzeType(SymbolAnalysisContext context, INamedTypeSymbol? unmanagedCallersOnly)
    {
        INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;

        if (type is { TypeKind: TypeKind.Delegate, DelegateInvokeMethod: { } invoke })
        {
            SpecialType inDelegate = InSignature(invoke, false);

            if (inDelegate != SpecialType.None)
                context.ReportDiagnostic(CreateDiagnostic(type, type.Name, inDelegate));

            return;
        }

        foreach (ISymbol member in type.GetMembers())
        {
            SpecialType found = member switch
            {
                IMethodSymbol method when Signatures.HasOwnSignature(method) => InSignature(method, IsCalledByNativeCode(method, unmanagedCallersOnly)),
                IPropertySymbol property => InTypes(TypesOf(property.Type, property.Parameters), false),
                IFieldSymbol { AssociatedSymbol: null } field => InCallbacksOf(field.Type),
                _ => SpecialType.None
            };

            if (found == SpecialType.None)
                continue;

            string name = member is IMethodSymbol constructor ? Signatures.NameOf(constructor) : member.Name;
            context.ReportDiagnostic(CreateDiagnostic(member, name, found));
        }
    }

    private static void AnalyzeLocalFunction(OperationAnalysisContext context, INamedTypeSymbol? unmanagedCallersOnly)
    {
        IMethodSymbol function = ((ILocalFunctionOperation)context.Operation).Symbol;

        if (function.ContainingSymbol is IMethodSymbol { PartialDefinitionPart: not null })
            return;

        SpecialType found = InSignature(function, IsCalledByNativeCode(function, unmanagedCallersOnly));

        if (found != SpecialType.None)
            context.ReportDiagnostic(CreateDiagnostic(function, function.Name, found));
    }

    private static SpecialType InSignature(IMethodSymbol signature, bool isCallback)
        => InTypes(TypesOf(signature.ReturnType, signature.Parameters), isCallback);

    private static SpecialType InTypes(IEnumerable<ITypeSymbol> types, bool isCallback)
    {
        foreach (ITypeSymbol type in types)
        {
            SpecialType found = isCallback && IsNotBlittable(type) ? type.SpecialType : InCallbacksOf(type);

            if (found != SpecialType.None)
                return found;
        }

        return SpecialType.None;

        static bool IsNotBlittable(ITypeSymbol type) => type.SpecialType is SpecialType.System_Boolean or SpecialType.System_Char;
    }

    private static SpecialType InCallbacksOf(ITypeSymbol type) => type switch
    {
        IPointerTypeSymbol pointer => InCallbacksOf(pointer.PointedAtType),
        IArrayTypeSymbol array => InCallbacksOf(array.ElementType),
        IFunctionPointerTypeSymbol function => InSignature(function.Signature, function.Signature.CallingConvention != SignatureCallingConvention.Default),
        _ => SpecialType.None
    };

    private static IEnumerable<ITypeSymbol> TypesOf(ITypeSymbol returned, ImmutableArray<IParameterSymbol> parameters)
    {
        yield return returned;

        foreach (IParameterSymbol parameter in parameters)
            yield return parameter.Type;
    }

    private static Diagnostic CreateDiagnostic(ISymbol symbol, string name, SpecialType found)
    {
        return Diagnostic.Create(Descriptors.InteropCallbackNotBlittable, symbol.Locations[0], name, KeywordOf(found), ReplacementOf(found));

        static string KeywordOf(SpecialType type) => type == SpecialType.System_Boolean ? "bool" : "char";

        static string ReplacementOf(SpecialType type) => type == SpecialType.System_Boolean ? "byte or int" : "byte or ushort";
    }

    private static bool IsCalledByNativeCode(IMethodSymbol method, INamedTypeSymbol? unmanagedCallersOnly)
        => unmanagedCallersOnly is not null && method.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, unmanagedCallersOnly));
}
