// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop.Helpers;

internal static class TypeLocations
{
    internal static Location Of(IParameterSymbol parameter, CancellationToken cancellationToken)
    {
        if (parameter.DeclaringSyntaxReferences is [SyntaxReference reference, ..] && reference.GetSyntax(cancellationToken) is ParameterSyntax { Type: { } type })
            return type.GetLocation();

        return parameter.Locations[0];
    }

    internal static Location OfReturn(IMethodSymbol method, CancellationToken cancellationToken)
    {
        SyntaxNode? declaration = method.DeclaringSyntaxReferences is [SyntaxReference reference, ..] ? reference.GetSyntax(cancellationToken) : null;

        Location? location = declaration switch
        {
            MethodDeclarationSyntax ordinary => ordinary.ReturnType.GetLocation(),
            OperatorDeclarationSyntax @operator => @operator.ReturnType.GetLocation(),
            ConversionOperatorDeclarationSyntax conversion => conversion.Type.GetLocation(),
            _ => null
        };

        return location ?? method.Locations[0];
    }
}
