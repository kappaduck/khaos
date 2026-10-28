// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InteropNonCopyableAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        Descriptors.InteropNonCopyableOutsideUsing,
        Descriptors.InteropNonCopyableCopied,
        Descriptors.InteropNonCopyableDisposedByHand
    ];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(static start =>
        {
            if (start.Compilation.GetTypeByMetadataName("KappaDuck.Khaos.Interop.NonCopyableAttribute") is not { } nonCopyable)
                return;

            start.RegisterOperationAction(context => AnalyzeCreation(context, nonCopyable), OperationKind.Invocation, OperationKind.ObjectCreation);
            start.RegisterOperationAction(context => AnalyzeReference(context, nonCopyable), OperationKind.LocalReference, OperationKind.ParameterReference);
        });
    }

    private static void AnalyzeCreation(OperationAnalysisContext context, INamedTypeSymbol nonCopyable)
    {
        IOperation creation = context.Operation;

        if (!IsNonCopyable(creation.Type, nonCopyable))
            return;

        IOperation owner = OutermostOf(creation);

        if (IsOwnedByUsing(owner.Parent) || owner.Parent is IReturnOperation)
            return;

        string type = creation.Type!.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropNonCopyableOutsideUsing, creation.Syntax.GetLocation(), type));
    }

    private static void AnalyzeReference(OperationAnalysisContext context, INamedTypeSymbol nonCopyable)
    {
        IOperation reference = context.Operation;

        if (!IsNonCopyable(reference.Type, nonCopyable))
            return;

        IOperation value = OutermostOf(reference);
        string name = NameOf(reference);

        switch (value.Parent)
        {
            case IInvocationOperation invocation when invocation.Instance == value:
                if (IsDispose(invocation.TargetMethod))
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropNonCopyableDisposedByHand, invocation.Syntax.GetLocation(), name));

                return;

            case IPropertyReferenceOperation property when property.Instance == value:
            case IFieldReferenceOperation field when field.Instance == value:
            case IMethodReferenceOperation method when method.Instance == value:
            case IForEachLoopOperation loop when loop.Collection == value:
            case IArgumentOperation { Parameter.RefKind: RefKind.In or RefKind.Ref or RefKind.RefReadOnlyParameter }:
            case ISimpleAssignmentOperation assignment when assignment.Target == value:
            case INameOfOperation:
                return;

            default:
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropNonCopyableCopied, reference.Syntax.GetLocation(), name));
                return;
        }
    }

    private static bool IsNonCopyable(ITypeSymbol? type, INamedTypeSymbol nonCopyable)
    {
        if (type is not INamedTypeSymbol named)
            return false;

        return named.OriginalDefinition.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, nonCopyable));
    }

    private static IOperation OutermostOf(IOperation operation)
    {
        IOperation current = operation;

        while (current.Parent is IConversionOperation { IsImplicit: true } or IParenthesizedOperation)
            current = current.Parent;

        return current;
    }

    private static bool IsOwnedByUsing(IOperation? parent)
    {
        if (parent is IUsingOperation)
            return true;

        if (parent is not IVariableInitializerOperation { Parent: IVariableDeclaratorOperation { Parent: IVariableDeclarationOperation { Parent: IVariableDeclarationGroupOperation group } } })
            return false;

        return group.Parent is IUsingDeclarationOperation or IUsingOperation;
    }

    private static bool IsDispose(IMethodSymbol method) => method is { Name: "Dispose", Parameters.IsEmpty: true };

    private static string NameOf(IOperation reference) => reference switch
    {
        ILocalReferenceOperation local => local.Local.Name,
        IParameterReferenceOperation parameter => parameter.Parameter.Name,
        _ => reference.Syntax.ToString()
    };
}
