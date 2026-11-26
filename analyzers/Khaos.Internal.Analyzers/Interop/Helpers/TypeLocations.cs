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

    internal static Location Of(IPropertySymbol property, CancellationToken cancellationToken)
    {
        SyntaxNode? declaration = property.DeclaringSyntaxReferences is [SyntaxReference reference, ..] ? reference.GetSyntax(cancellationToken) : null;

        Location? location = declaration switch
        {
            BasePropertyDeclarationSyntax ordinary => ordinary.Type.GetLocation(),
            ParameterSyntax { Type: { } positional } => positional.GetLocation(),
            _ => null
        };

        return location ?? property.Locations[0];
    }

    internal static Location Of(IEventSymbol @event, CancellationToken cancellationToken)
    {
        SyntaxNode? declaration = @event.DeclaringSyntaxReferences is [SyntaxReference reference, ..] ? reference.GetSyntax(cancellationToken) : null;

        Location? location = declaration switch
        {
            EventDeclarationSyntax accessors => accessors.Type.GetLocation(),
            VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax field } => field.Type.GetLocation(),
            _ => null
        };

        return location ?? @event.Locations[0];
    }

    internal static Location Of(IFieldSymbol field, CancellationToken cancellationToken)
    {
        if (field.DeclaringSyntaxReferences is [SyntaxReference reference, ..] && reference.GetSyntax(cancellationToken).Parent is VariableDeclarationSyntax declaration)
            return declaration.Type.GetLocation();

        return field.Locations[0];
    }

    internal static Location OfReturn(IMethodSymbol method, CancellationToken cancellationToken)
    {
        SyntaxNode? declaration = method.DeclaringSyntaxReferences is [SyntaxReference reference, ..] ? reference.GetSyntax(cancellationToken) : null;

        Location? location = declaration switch
        {
            MethodDeclarationSyntax ordinary => ordinary.ReturnType.GetLocation(),
            OperatorDeclarationSyntax @operator => @operator.ReturnType.GetLocation(),
            ConversionOperatorDeclarationSyntax conversion => conversion.Type.GetLocation(),
            DelegateDeclarationSyntax @delegate => @delegate.ReturnType.GetLocation(),
            LocalFunctionStatementSyntax local => local.ReturnType.GetLocation(),
            _ => null
        };

        return location ?? method.Locations[0];
    }
}
