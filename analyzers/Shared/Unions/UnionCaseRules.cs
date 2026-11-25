// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace KappaDuck.Khaos.Analyzers.Shared.Unions;

internal static class UnionCaseRules
{
    internal static IFieldSymbol? FindReferenceField(INamedTypeSymbol type)
    {
        if (type.IsUnmanagedType)
            return null;

        foreach (IFieldSymbol field in type.GetMembers().OfType<IFieldSymbol>())
        {
            if (field.IsStatic || field.IsConst)
                continue;

            if (field.Type.SpecialType != SpecialType.System_String && !field.Type.IsUnmanagedType)
                return field;
        }

        return null;
    }
}
