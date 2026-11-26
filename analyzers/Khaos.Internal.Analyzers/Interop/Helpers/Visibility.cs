// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop.Helpers;

internal static class Visibility
{
    internal static bool LeavesTheAssembly(ISymbol symbol)
    {
        ISymbol? current = symbol;

        while (current is not null)
        {
            if (current.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal))
                return false;

            current = current.ContainingType;
        }

        return true;
    }
}
