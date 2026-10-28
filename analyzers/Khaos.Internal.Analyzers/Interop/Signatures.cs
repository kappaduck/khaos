// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

internal static class Signatures
{
    internal static bool HasOwn(IMethodSymbol method) => method is
    {
        IsImplicitlyDeclared: false,
        PartialDefinitionPart: null,
        MethodKind: MethodKind.Ordinary or MethodKind.Constructor or MethodKind.UserDefinedOperator or MethodKind.Conversion
    };

    internal static bool Contains(IMethodSymbol signature, Func<ITypeSymbol, bool> match)
        => Contains(signature.ReturnType, match) || signature.Parameters.Any(parameter => Contains(parameter.Type, match));

    internal static bool Contains(ITypeSymbol type, Func<ITypeSymbol, bool> match)
    {
        if (match(type))
            return true;

        return type switch
        {
            IPointerTypeSymbol pointer => Contains(pointer.PointedAtType, match),
            IFunctionPointerTypeSymbol function => Contains(function.Signature, match),
            _ => false
        };
    }
}
