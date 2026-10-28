// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

internal static class TypeLocations
{
    internal static Location Of(IPropertySymbol property, CancellationToken cancellationToken)
    {
        if (property.DeclaringSyntaxReferences is [var reference, ..] && reference.GetSyntax(cancellationToken) is BasePropertyDeclarationSyntax declaration)
            return declaration.Type.GetLocation();

        return property.Locations[0];
    }

    internal static Location Of(IFieldSymbol field, CancellationToken cancellationToken)
    {
        if (field.DeclaringSyntaxReferences is [var reference, ..] && reference.GetSyntax(cancellationToken).Parent is VariableDeclarationSyntax declaration)
            return declaration.Type.GetLocation();

        return field.Locations[0];
    }
}
