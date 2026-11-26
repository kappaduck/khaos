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
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        Descriptors.InteropNonCopyableOutsideUsing,
        Descriptors.InteropNonCopyableCopied,
        Descriptors.InteropNonCopyableDisposedByHand
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(static start =>
        {
            if (start.Compilation.GetTypeByMetadataName("KappaDuck.Khaos.Interop.NonCopyableAttribute") is not { } nonCopyable)
                return;

            start.RegisterOperationAction(ctx => AnalyzeCreation(ctx, nonCopyable), OperationKind.Invocation, OperationKind.ObjectCreation);
            start.RegisterOperationAction(ctx => AnalyzeReference(ctx, nonCopyable), OperationKind.LocalReference, OperationKind.ParameterReference);
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
            case IIsPatternOperation pattern when pattern.Value == value && !Binds(pattern.Pattern):
            case ISwitchExpressionOperation expression when expression.Value == value && !expression.Arms.Any(static arm => Binds(arm.Pattern)):
            case ISwitchOperation statement when !PatternsOf(statement).Any(Binds):
            case INameOfOperation:
                return;

            default:
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropNonCopyableCopied, reference.Syntax.GetLocation(), name));
                return;
        }

        static bool IsDispose(IMethodSymbol method) => method is { Name: "Dispose", Parameters.IsEmpty: true };

        static string NameOf(IOperation reference)
            => reference is ILocalReferenceOperation local ? local.Local.Name : ((IParameterReferenceOperation)reference).Parameter.Name;
    }

    private static bool Binds(IPatternOperation pattern) => pattern switch
    {
        IDeclarationPatternOperation { DeclaredSymbol: not null } or IRecursivePatternOperation { DeclaredSymbol: not null } => true,
        IBinaryPatternOperation binary => Binds(binary.LeftPattern) || Binds(binary.RightPattern),
        _ => false
    };

    private static bool IsNonCopyable(ITypeSymbol? type, INamedTypeSymbol nonCopyable)
    {
        if (type is not INamedTypeSymbol named)
            return false;

        return named.OriginalDefinition.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, nonCopyable));
    }

    private static IOperation OutermostOf(IOperation operation)
    {
        IOperation current = operation;

        while (current.Parent is { } parent && Carries(parent, current))
            current = parent;

        return current;
    }

    private static bool Carries(IOperation parent, IOperation value) => parent switch
    {
        IConversionOperation { IsImplicit: true } => true,
        IConditionalOperation conditional => conditional.WhenTrue == value || conditional.WhenFalse == value,
        ISwitchExpressionArmOperation arm => arm.Value == value,
        ISwitchExpressionOperation => value is ISwitchExpressionArmOperation,
        _ => false
    };

    private static bool IsOwnedByUsing(IOperation? parent)
    {
        if (parent is IUsingOperation)
            return true;

        if (parent is not IVariableInitializerOperation { Parent: IVariableDeclaratorOperation { Parent: IVariableDeclarationOperation { Parent: IVariableDeclarationGroupOperation group } } })
            return false;

        return group.Parent is IUsingDeclarationOperation or IUsingOperation;
    }

    private static IEnumerable<IPatternOperation> PatternsOf(ISwitchOperation statement)
        => statement.Cases.SelectMany(static section => section.Clauses).OfType<IPatternCaseClauseOperation>().Select(static clause => clause.Pattern);
}
