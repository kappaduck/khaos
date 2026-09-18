// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace KappaDuck.Khaos.Internal.Analyzers;

internal static class Descriptors
{
    private const string Documentation = "https://github.com/kappaduck/khaos/blob/main/docs/rules/";
    private const string Interop = "Interop";

    internal static readonly DiagnosticDescriptor BooleanMarshalling = new(
        id: "KHI001",
        title: "bool does not follow the marshalling convention of its interop area",
        messageFormat: "{0} does not follow the {1} bool convention; expected [{2}]",
        category: Interop,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "SDL3 uses the 1-byte C99 bool, so every bool crossing into Interop/SDL needs UnmanagedType.U1. Win32 uses the 4-byte BOOL, so Interop/Win32 needs UnmanagedType.Bool.",
        helpLinkUri: Documentation + "khi001.md");

    internal static readonly DiagnosticDescriptor BooleanField = new(
        id: "KHI009",
        title: "bool in an interop struct",
        messageFormat: "{0} is declared as bool; in {1} a native boolean is stored as {2}",
        category: Interop,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "bool is not blittable, so a struct that holds one requires marshalling: the LibraryImport generator refuses to pass it by value, and [MarshalAs] on the field does not change that. On a struct that is only reinterpreted through a pointer the attribute does nothing at all. Storing the field at its native width keeps the struct blittable and the layout honest.",
        helpLinkUri: Documentation + "khi009.md");
}
