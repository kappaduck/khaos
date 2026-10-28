// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace KappaDuck.Khaos.Internal.Analyzers;

internal static class Descriptors
{
    private const string Documentation = "https://github.com/kappaduck/khaos/blob/main/docs/rules/";
    private const string Interop = "Interop";
    private const string InteropLibraryDescription = "The other interop rules take their conventions from the area, so a binding in the wrong area is checked against the wrong ones. A library no area declares has not been decided on; add it to its area in InteropAreas first.";

    internal static readonly DiagnosticDescriptor InteropVisibility = new(
        "KHI005",
        "type in Interop is not internal",
        "'{0}' is visible outside the assembly; every type in Interop is internal",
        Interop,
        DiagnosticSeverity.Error,
        true,
        "Once every interop type is internal, the compiler refuses any public signature that mentions one, so the boundary holds without walking signatures. A type that has to be seen from outside does not belong in Interop.",
        $"{Documentation}khi005.md");

    internal static readonly DiagnosticDescriptor InteropLibraryWrongArea = new(
    "KHI006",
    "native library does not match its interop area",
    "'{0}' does not belong to {1}; it belongs to {2}",
    Interop,
    DiagnosticSeverity.Error,
    true,
    InteropLibraryDescription,
    $"{Documentation}khi006.md");

    internal static readonly DiagnosticDescriptor InteropLibraryUnknown = new(
        "KHI006",
        "native library does not match its interop area",
        "'{0}' does not belong to {1}; no interop area declares it",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropLibraryDescription,
        $"{Documentation}khi006.md");
}
