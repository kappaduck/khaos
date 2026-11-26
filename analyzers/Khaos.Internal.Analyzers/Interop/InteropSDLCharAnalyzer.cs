// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InteropSDLCharAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        Descriptors.InteropSDLCharReturn,
        Descriptors.InteropSDLCharParameter,
        Descriptors.InteropSDLCharField,
        Descriptors.InteropSDLCharProperty,
        Descriptors.InteropSDLCharEvent
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
        context.RegisterOperationAction(AnalyzeLocalFunction, OperationKind.LocalFunction);
    }

    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;

        if (!IsInSDL(type))
            return;

        if (type is { TypeKind: TypeKind.Delegate, DelegateInvokeMethod: { } invoke })
        {
            AnalyzeMethod(invoke, context.ReportDiagnostic, context.CancellationToken);
            return;
        }

        foreach (ISymbol member in type.GetMembers())
        {
            if (member is IMethodSymbol method && Signatures.HasOwnSignature(method))
            {
                AnalyzeMethod(method, context.ReportDiagnostic, context.CancellationToken);
            }
            else if (member is IFieldSymbol { IsConst: false, AssociatedSymbol: null } field && UsesChar(field.Type) && !Fields.TryGetCapturedParameter(field, out _))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropSDLCharField, TypeLocations.Of(field, context.CancellationToken), type.Name, field.Name));
            }
            else if (member is IPropertySymbol property && UsesChar(property.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropSDLCharProperty, TypeLocations.Of(property, context.CancellationToken), type.Name, property.Name));
            }
            else if (member is IEventSymbol @event && UsesChar(@event.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropSDLCharEvent, TypeLocations.Of(@event, context.CancellationToken), type.Name, @event.Name));
            }
        }
    }

    private static void AnalyzeLocalFunction(OperationAnalysisContext context)
    {
        IMethodSymbol function = ((ILocalFunctionOperation)context.Operation).Symbol;

        if (function.ContainingSymbol is IMethodSymbol { PartialDefinitionPart: not null } || !IsInSDL(function))
            return;

        AnalyzeMethod(function, context.ReportDiagnostic, context.CancellationToken);
    }

    private static bool IsInSDL(ISymbol symbol) => InteropAreas.TryResolve(symbol, out InteropArea area) && area == InteropArea.SDL;

    private static void AnalyzeMethod(IMethodSymbol method, Action<Diagnostic> report, CancellationToken cancellationToken)
    {
        string name = Signatures.NameOf(method);

        if (UsesChar(method.ReturnType))
            report(Diagnostic.Create(Descriptors.InteropSDLCharReturn, TypeLocations.OfReturn(method, cancellationToken), name));

        foreach (IParameterSymbol parameter in method.Parameters.Where(static parameter => UsesChar(parameter.Type)))
            report(Diagnostic.Create(Descriptors.InteropSDLCharParameter, TypeLocations.Of(parameter, cancellationToken), parameter.Name, name));
    }

    private static bool UsesChar(ITypeSymbol type) => type switch
    {
        { SpecialType: SpecialType.System_Char } => true,
        IPointerTypeSymbol pointer => UsesChar(pointer.PointedAtType),
        IArrayTypeSymbol array => UsesChar(array.ElementType),
        IFunctionPointerTypeSymbol function => UsesChar(function.Signature.ReturnType) || function.Signature.Parameters.Any(static parameter => UsesChar(parameter.Type)),
        INamedTypeSymbol { IsGenericType: true } generic => generic.TypeArguments.Any(UsesChar),
        _ => false
    };
}
