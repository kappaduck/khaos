// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop.Helpers;

internal static class InteropAreas
{
    private static readonly FrozenDictionary<string, InteropArea> _libraries = new Dictionary<string, InteropArea>
    {
        { "SDL3", InteropArea.SDL },
        { "SDL3_image", InteropArea.SDL },
        { "SDL3_ttf", InteropArea.SDL },
        { "SDL3_mixer", InteropArea.SDL },
        { "user32", InteropArea.Win32 },
        { "kernel32", InteropArea.Win32 },
        { "gdi32", InteropArea.Win32 }
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenDictionary<string, string> _prefixes = new Dictionary<string, string>
    {
        { "SDL3", "SDL_" },
        { "SDL3_image", "IMG_" },
        { "SDL3_ttf", "TTF_" },
        { "SDL3_mixer", "MIX_" }
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    internal static UnmanagedType GetBoolConvention(InteropArea area) => area switch
    {
        InteropArea.SDL => UnmanagedType.U1,
        InteropArea.Win32 => UnmanagedType.Bool,
        _ => throw new ArgumentOutOfRangeException(nameof(area), area, null)
    };

    internal static bool IsInInterop(ISymbol symbol)
    {
        INamespaceSymbol current = symbol.ContainingNamespace;

        while (current is { IsGlobalNamespace: false })
        {
            if (IsInteropRoot(current))
                return true;

            current = current.ContainingNamespace;
        }

        return false;
    }

    internal static bool TryGetOwner(string library, out InteropArea area) => _libraries.TryGetValue(library, out area);

    internal static bool TryGetPrefix(string library, [NotNullWhen(true)] out string? prefix) => _prefixes.TryGetValue(library, out prefix);

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
