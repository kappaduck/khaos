// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InteropNintAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        Descriptors.InteropNintReturn,
        Descriptors.InteropNintParameter,
        Descriptors.InteropNintField,
        Descriptors.InteropNintProperty
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(static start =>
        {
            INamedTypeSymbol? inlineArray = start.Compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.InlineArrayAttribute");
            start.RegisterSymbolAction(ctx => AnalyzeType(ctx, inlineArray), SymbolKind.NamedType);
        });
    }

    private static void AnalyzeType(SymbolAnalysisContext context, INamedTypeSymbol? inlineArray)
    {
        INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;

        if (!InteropAreas.TryResolve(type, out _) || IsWrappedNint(type, inlineArray))
            return;

        if (type is { TypeKind: TypeKind.Delegate, DelegateInvokeMethod: { } invoke })
        {
            AnalyzeMethod(context, invoke);
            return;
        }

        foreach (ISymbol member in type.GetMembers())
        {
            if (member is IMethodSymbol method && Signatures.HasOwnSignature(method))
            {
                AnalyzeMethod(context, method);
            }
            else if (member is IFieldSymbol { IsConst: false, AssociatedSymbol: null } field && UsesNint(field.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropNintField, TypeLocations.Of(field, context.CancellationToken), type.Name, field.Name));
            }
            else if (member is IPropertySymbol property && UsesNint(property.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropNintProperty, TypeLocations.Of(property, context.CancellationToken), type.Name, property.Name));
            }
        }
    }

    private static bool IsWrappedNint(INamedTypeSymbol type, INamedTypeSymbol? inlineArray)
    {
        if (type.TypeKind is not TypeKind.Struct || IsInlineArray(type, inlineArray))
            return false;

        IFieldSymbol[] fields = [.. type.GetMembers().OfType<IFieldSymbol>().Where(static f => !f.IsStatic)];
        return fields is [{ Type.SpecialType: SpecialType.System_IntPtr }];
    }

    private static bool IsInlineArray(INamedTypeSymbol type, INamedTypeSymbol? inlineArray)
    {
        return inlineArray is not null &&
               type.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, inlineArray));
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context, IMethodSymbol method)
    {
        string name = Signatures.NameOf(method);

        if (UsesNint(method.ReturnType))
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropNintReturn, TypeLocations.OfReturn(method, context.CancellationToken), name));

        foreach (IParameterSymbol parameter in method.Parameters.Where(static parameter => UsesNint(parameter.Type)))
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropNintParameter, TypeLocations.Of(parameter, context.CancellationToken), parameter.Name, name));
    }

    private static bool UsesNint(ITypeSymbol type)
        => Signatures.Contains(type, static candidate => candidate.SpecialType == SpecialType.System_IntPtr);
}
