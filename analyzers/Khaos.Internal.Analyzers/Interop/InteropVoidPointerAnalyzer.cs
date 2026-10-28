// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InteropVoidPointerAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptors.InteropVoidPointerUnexplained, Descriptors.InteropVoidPointerEmptyReason];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(static start =>
        {
            INamedTypeSymbol? untypedBecause = start.Compilation.GetTypeByMetadataName("KappaDuck.Khaos.Interop.UntypedBecauseAttribute");
            start.RegisterSymbolAction(context => AnalyzeMethod(context, untypedBecause), SymbolKind.Method);
        });
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context, INamedTypeSymbol? untypedBecause)
    {
        IMethodSymbol method = (IMethodSymbol)context.Symbol;

        if (InteropAreas.Resolve(method) is null || !Signatures.HasOwn(method) || !Signatures.Contains(method, IsVoidPointer))
            return;

        string name = Signatures.NameOf(method);
        AttributeData? reason = method.GetAttributes().FirstOrDefault(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, untypedBecause));

        if (reason is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropVoidPointerUnexplained, method.Locations[0], name));
            return;
        }

        if (reason is { ConstructorArguments: [{ Value: string sentence }] } && !string.IsNullOrWhiteSpace(sentence))
            return;

        Location location = ReasonLocationOf(reason, context.CancellationToken) ?? method.Locations[0];
        context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropVoidPointerEmptyReason, location, name));
    }

    private static bool IsVoidPointer(ITypeSymbol type) => type is IPointerTypeSymbol { PointedAtType.SpecialType: SpecialType.System_Void };

    private static Location? ReasonLocationOf(AttributeData reason, CancellationToken cancellationToken)
    {
        if (reason.ApplicationSyntaxReference?.GetSyntax(cancellationToken) is AttributeSyntax { ArgumentList.Arguments: [AttributeArgumentSyntax sentence, ..] })
            return sentence.GetLocation();

        return null;
    }
}
