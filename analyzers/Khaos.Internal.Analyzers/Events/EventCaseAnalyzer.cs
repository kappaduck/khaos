// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Events;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class EventCaseAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
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
        AttributeData? marker = type.GetAttributes().FirstOrDefault(d => SymbolEqualityComparer.Default.Equals(d.AttributeClass, attribute));

        if (marker is null)
            return;

        Location location = type.Locations[0];
        EventCaseProblem? problem = EventCaseRules.Check(type, marker, context.Compilation, context.CancellationToken, out EventCase? @case);

        if (problem is not null)
        {
            context.ReportDiagnostic(Diagnostic.Create(problem.Descriptor, location, problem.Arguments));
            return;
        }

        if (@case is not null)
            cases.Add((@case, location));
    }

    private static void ReportConflicts(CompilationAnalysisContext context, ConcurrentBag<(EventCase Case, Location Location)> cases)
    {
        Dictionary<string, Location> locations = [with(StringComparer.Ordinal)];

        foreach ((EventCase @case, Location location) in cases)
            locations[@case.TypeName] = location;

        List<(EventCase Dropped, EventCase Kept)> conflicts = [];
        _ = EventUnion.Resolve(cases.Select(static entry => entry.Case), conflicts);

        foreach ((EventCase dropped, EventCase kept) in conflicts)
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.EventCaseDuplicate, locations[dropped.TypeName], dropped.Name, kept.Name));
    }
}
