// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InteropVoidPointerAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        Descriptors.InteropVoidPointerUnexplained,
        Descriptors.InteropVoidPointerEmptyReason
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(static start =>
        {
            INamedTypeSymbol? untypedBecause = start.Compilation.GetTypeByMetadataName("KappaDuck.Khaos.Interop.UntypedBecauseAttribute");

            start.RegisterSymbolAction(context => AnalyzeMethod(context, untypedBecause), SymbolKind.Method);
            start.RegisterSymbolAction(context => AnalyzeDelegate(context, untypedBecause), SymbolKind.NamedType);
        });
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context, INamedTypeSymbol? untypedBecause)
    {
        IMethodSymbol method = (IMethodSymbol)context.Symbol;

        if (Signatures.HasOwnSignature(method))
            Analyze(context, method, method, untypedBecause);
    }

    private static void AnalyzeDelegate(SymbolAnalysisContext context, INamedTypeSymbol? untypedBecause)
    {
        if (context.Symbol is INamedTypeSymbol { TypeKind: TypeKind.Delegate, DelegateInvokeMethod: { } invoke } type)
            Analyze(context, type, invoke, untypedBecause);
    }

    private static void Analyze(SymbolAnalysisContext context, ISymbol declaration, IMethodSymbol signature, INamedTypeSymbol? untypedBecause)
    {
        if (!InteropAreas.TryResolve(declaration, out _) || !Signatures.Contains(signature, IsVoidPointer))
            return;

        string name = Signatures.NameOf(signature);
        AttributeData? reason = declaration.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, untypedBecause));

        if (reason is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropVoidPointerUnexplained, declaration.Locations[0], name));
            return;
        }

        if (reason is { ConstructorArguments: [{ Value: string sentence }] } && !string.IsNullOrWhiteSpace(sentence))
            return;

        Location location = ReasonLocationOf(reason, context.CancellationToken) ?? declaration.Locations[0];
        context.ReportDiagnostic(Diagnostic.Create(Descriptors.InteropVoidPointerEmptyReason, location, name));

        static bool IsVoidPointer(ITypeSymbol type) => type is IPointerTypeSymbol { PointedAtType.SpecialType: SpecialType.System_Void };
    }

    private static Location? ReasonLocationOf(AttributeData reason, CancellationToken cancellationToken)
    {
        if (reason.ApplicationSyntaxReference?.GetSyntax(cancellationToken) is AttributeSyntax { ArgumentList.Arguments: [AttributeArgumentSyntax sentence, ..] })
            return sentence.GetLocation();

        return null;
    }
}
