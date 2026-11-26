// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Events.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Events;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EventCaseAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        Descriptors.EventCaseShape,
        Descriptors.EventCaseRange,
        Descriptors.EventCaseField,
        Descriptors.EventCaseConstructor,
        Descriptors.EventCaseDuplicate
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(static start =>
        {
            INamedTypeSymbol? attribute = start.Compilation.GetTypeByMetadataName(EventCaseRules.AttributeName);

            if (attribute is null)
                return;

            ConcurrentBag<(EventCase Case, Location Location)> cases = [];

            start.RegisterSymbolAction(context => Analyze(context, attribute, cases), SymbolKind.NamedType);
            start.RegisterCompilationEndAction(context => ReportConflicts(context, cases));
        });
    }

    private static void Analyze(SymbolAnalysisContext context, INamedTypeSymbol attribute, ConcurrentBag<(EventCase Case, Location Location)> cases)
    {
        INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;
        AttributeData? marker = type.GetAttributes().FirstOrDefault(data => SymbolEqualityComparer.Default.Equals(data.AttributeClass, attribute));

        if (marker is null)
            return;

        List<Diagnostic> problems = [];
        EventCase? @case = EventCaseRules.Check(type, marker, context.Compilation, problems, context.CancellationToken);

        foreach (Diagnostic problem in problems)
            context.ReportDiagnostic(problem);

        if (@case is not null)
            cases.Add((@case, EventCaseRules.GetLocation(type, marker, context.CancellationToken)));
    }

    private static void ReportConflicts(CompilationAnalysisContext context, ConcurrentBag<(EventCase Case, Location Location)> cases)
    {
        Dictionary<string, Location> locations = [with(StringComparer.Ordinal)];

        foreach ((EventCase @case, Location location) in cases)
            locations[@case.Union.TypeName] = location;

        List<(EventCase Dropped, EventCase Kept)> conflicts = [];
        _ = EventUnion.Resolve(cases.Select(static entry => entry.Case), conflicts);

        foreach ((EventCase dropped, EventCase kept) in conflicts)
            context.ReportDiagnostic(EventCaseRules.Duplicate(dropped, kept, locations[dropped.Union.TypeName]));
    }
}
