// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Analyzers.Hosting;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedundantWindowAnalyzer : DiagnosticAnalyzer
{
    private const string GameName = "KappaDuck.Khaos.Game2D";
    private const string WindowName = "KappaDuck.Khaos.Windowing.Window";

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptors.RedundantWindow];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(static start =>
        {
            INamedTypeSymbol? game = start.Compilation.GetTypeByMetadataName(GameName);
            INamedTypeSymbol? window = start.Compilation.GetTypeByMetadataName(WindowName);

            if (game is not null && window is not null)
                start.RegisterSymbolAction(context => Analyze(context, game, window), SymbolKind.Method);
        });
    }

    private static void Analyze(SymbolAnalysisContext context, INamedTypeSymbol game, INamedTypeSymbol window)
    {
        IMethodSymbol constructor = (IMethodSymbol)context.Symbol;

        if (constructor.MethodKind != MethodKind.Constructor || !IsGame(constructor.ContainingType, game))
            return;

        foreach (IParameterSymbol parameter in constructor.Parameters)
        {
            if (!SymbolEqualityComparer.Default.Equals(parameter.Type, window))
                continue;

            foreach (Location location in parameter.Locations)
            {
                if (location.IsInSource)
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.RedundantWindow, location, constructor.ContainingType.Name, parameter.Name));
            }
        }
    }

    private static bool IsGame(INamedTypeSymbol type, INamedTypeSymbol game)
    {
        for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, game))
                return true;
        }

        return false;
    }
}
