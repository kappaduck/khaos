// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop.Helpers;

internal static class InteropAreas
{
    internal static UnmanagedType GetBoolConvention(InteropArea area) => area switch
    {
        InteropArea.SDL => UnmanagedType.U1,
        InteropArea.Win32 => UnmanagedType.Bool,
        _ => throw new ArgumentOutOfRangeException(nameof(area), area, null)
    };

    internal static bool TryResolve(ISymbol symbol, [NotNullWhen(true)] out InteropArea area)
    {
        string? @namespace = null;
        INamespaceSymbol current = symbol.ContainingNamespace;

        while (current is { IsGlobalNamespace: false })
        {
            if (IsInteropRoot(current))
                return TryParse(@namespace, out area);

            @namespace = current.Name;
            current = current.ContainingNamespace;
        }

        area = default;
        return false;
    }

    private static bool IsInteropRoot(INamespaceSymbol candidate)
    {
        return candidate is
        {
            Name: "Interop",
            ContainingNamespace:
            {
                Name: "Khaos",
                ContainingNamespace:
                {
                    Name: "KappaDuck",
                    ContainingNamespace.IsGlobalNamespace: true
                }
            }
        };
    }

    private static bool TryParse(string? name, out InteropArea area)
    {
        switch (name)
        {
            case "SDL":
                area = InteropArea.SDL;
                return true;

            case "Win32":
                area = InteropArea.Win32;
                return true;

            default:
                area = default;
                return false;
        }
    }
}
