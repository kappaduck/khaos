// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using System.Collections.Frozen;
using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

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

    internal static InteropArea? OwnerOf(string library)
        => _libraries.TryGetValue(library, out InteropArea area) ? area : null;

    internal static UnmanagedType BoolConvention(InteropArea area) => area switch
    {
        InteropArea.SDL => UnmanagedType.U1,
        InteropArea.Win32 => UnmanagedType.Bool,
        _ => throw new ArgumentOutOfRangeException(nameof(area), area, null)
    };

    internal static string BoolStorage(InteropArea area) => area switch
    {
        InteropArea.SDL => "byte",
        InteropArea.Win32 => "int",
        _ => throw new ArgumentOutOfRangeException(nameof(area), area, null)
    };

    private static InteropArea? Parse(string? name) => name switch
    {
        "SDL" => InteropArea.SDL,
        "Win32" => InteropArea.Win32,
        _ => null
    };

    private static bool IsInteropRoot(INamespaceSymbol candidate)
        => candidate is { Name: "Interop", ContainingNamespace: { Name: "Khaos", ContainingNamespace: { Name: "KappaDuck", ContainingNamespace.IsGlobalNamespace: true } } };
}
