// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

internal static class InteropAreas
{
    private const string Company = "KappaDuck";
    private const string Product = "Khaos";
    private const string Root = "Interop";
    private const string Sdl = "SDL";
    private const string Win32 = "Win32";

    internal static InteropArea? Resolve(ISymbol symbol)
    {
        INamespaceSymbol? current = symbol.ContainingNamespace;
        string? area = null;

        while (current is { IsGlobalNamespace: false })
        {
            if (IsInteropRoot(current))
                return Area(area);

            area = current.Name;
            current = current.ContainingNamespace;
        }

        return null;
    }

    extension(InteropArea area)
    {
        internal UnmanagedType Convention()
        {
            if (area == InteropArea.Sdl)
                return UnmanagedType.U1;

            if (area == InteropArea.Win32)
                return UnmanagedType.Bool;

            throw new ArgumentOutOfRangeException(nameof(area));
        }

        internal string ConventionName()
        {
            if (area == InteropArea.Sdl)
                return nameof(UnmanagedType.U1);

            if (area == InteropArea.Win32)
                return nameof(UnmanagedType.Bool);

            throw new ArgumentOutOfRangeException(nameof(area));
        }

        internal string FieldType => area switch
        {
            InteropArea.Sdl => "byte",
            InteropArea.Win32 => "int",
            _ => throw new ArgumentOutOfRangeException(nameof(area))
        };

        internal string Name => area switch
        {
            InteropArea.Sdl => Sdl,
            InteropArea.Win32 => Win32,
            _ => throw new ArgumentOutOfRangeException(nameof(area))
        };
    }

    private static InteropArea? Area(string? name) => name switch
    {
        Sdl => InteropArea.Sdl,
        Win32 => InteropArea.Win32,
        _ => null
    };

    private static bool IsInteropRoot(INamespaceSymbol candidate)
        => candidate.Name == Root
        && candidate.ContainingNamespace?.Name == Product
        && candidate.ContainingNamespace.ContainingNamespace?.Name == Company;
}
