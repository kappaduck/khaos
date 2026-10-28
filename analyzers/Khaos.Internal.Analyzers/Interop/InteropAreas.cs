// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

internal static class InteropAreas
{
    internal static InteropArea? Resolve(ISymbol symbol)
    {
        string? area = null;
        for (INamespaceSymbol current = symbol.ContainingNamespace; current is { IsGlobalNamespace: false }; current = current.ContainingNamespace)
        {
            if (IsInteropRoot(current))
                return Parse(area);

            area = current.Name;
        }

        return null;
    }

    internal static bool IsInInterop(ISymbol symbol)
    {
        for (INamespaceSymbol current = symbol.ContainingNamespace; current is { IsGlobalNamespace: false }; current = current.ContainingNamespace)
        {
            if (IsInteropRoot(current))
                return true;
        }

        return false;
    }

    private static InteropArea? Parse(string? name) => name switch
    {
        "SDL" => InteropArea.SDL,
        "Win32" => InteropArea.Win32,
        _ => null
    };

    private static bool IsInteropRoot(INamespaceSymbol candidate)
        => candidate is { Name: "Interop", ContainingNamespace: { Name: "Khaos", ContainingNamespace: { Name: "KappaDuck", ContainingNamespace.IsGlobalNamespace: true } } };
}
