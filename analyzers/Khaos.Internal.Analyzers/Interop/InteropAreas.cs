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

    private static readonly string[] _sdlLibraries = ["SDL3", "SDL3_image", "SDL3_ttf", "SDL3_mixer"];
    private static readonly string[] _win32Libraries = ["user32", "kernel32", "gdi32"];

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

    internal static InteropArea? ResolveLibrary(string library)
    {
        if (Declares(_sdlLibraries, library))
            return InteropArea.Sdl;

        if (Declares(_win32Libraries, library))
            return InteropArea.Win32;

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

        internal string ExampleType => area switch
        {
            InteropArea.Sdl => "SDL_Window*",
            InteropArea.Win32 => "HWND*",
            _ => throw new ArgumentOutOfRangeException(nameof(area))
        };

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

    private static bool Declares(string[] libraries, string library)
        => Array.Exists(libraries, name => string.Equals(name, library, StringComparison.OrdinalIgnoreCase));

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
