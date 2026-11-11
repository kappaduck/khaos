// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Analyzers.Events;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CustomEventsAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        Descriptors.EventGroupShape,
        Descriptors.EventGroupTooLarge,
        Descriptors.EventGroupEmpty,
        Descriptors.EventShape,
        Descriptors.EventGroupName,
        Descriptors.EventDuplicate
    ];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(static start =>
        {
            INamedTypeSymbol? attribute = start.Compilation.GetTypeByMetadataName(CustomEventRules.AttributeName);

            if (attribute is not null)
                start.RegisterSymbolAction(context => Analyze(context, attribute), SymbolKind.NamedType);
        });
    }

    private static void Analyze(SymbolAnalysisContext context, INamedTypeSymbol attribute)
    {
        INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;

        if (!type.GetAttributes().Any(data => SymbolEqualityComparer.Default.Equals(data.AttributeClass, attribute)))
            return;

        List<CustomEventProblem> problems = [];
        _ = CustomEventRules.Check(type, context.Compilation, problems, context.CancellationToken);

        foreach (CustomEventProblem problem in problems)
            context.ReportDiagnostic(Diagnostic.Create(problem.Descriptor, problem.Location, problem.Arguments));
    }
}
