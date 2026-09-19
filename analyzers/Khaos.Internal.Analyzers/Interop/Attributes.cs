// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

internal static class Attributes
{
    internal static AttributeData? Find(ImmutableArray<AttributeData> attributes, ImmutableArray<INamedTypeSymbol> candidates)
        => attributes.FirstOrDefault(attribute => attribute.AttributeClass is not null && candidates.Contains(attribute.AttributeClass, SymbolEqualityComparer.Default));

    internal static Location Location(AttributeData attribute, SyntaxNode fallback, CancellationToken cancellationToken)
    {
        if (attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken) is AttributeSyntax syntax)
            return syntax.GetLocation();

        return fallback.GetLocation();
    }
}
