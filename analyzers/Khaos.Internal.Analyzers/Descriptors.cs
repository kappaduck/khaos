// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace KappaDuck.Khaos.Internal.Analyzers;

internal static class Descriptors
{
    private const string Documentation = "https://github.com/kappaduck/khaos/blob/main/docs/rules/";
    private const string Interop = "Interop";

    internal static readonly DiagnosticDescriptor BooleanMarshalling = new(
        "KHI001",
        "bool does not follow the marshalling convention of its interop area",
        "{0} does not follow the {1} bool convention; expected [{2}]",
        Interop,
        DiagnosticSeverity.Error,
        true,
        "SDL3 uses the 1-byte C99 bool, so every bool crossing into Interop/SDL needs UnmanagedType.U1. Win32 uses the 4-byte BOOL, so Interop/Win32 needs UnmanagedType.Bool.",
        Documentation + "khi001.md");

    internal static readonly DiagnosticDescriptor NativeInteger = new(
        "KHI002",
        "nint or IntPtr in an interop declaration",
        "{0} is declared as nint; name the native type it carries, such as {1}",
        Interop,
        DiagnosticSeverity.Error,
        true,
        "nint and System.IntPtr are the same type, and an nint compiles wherever any other handle is expected: the compiler cannot tell a window from a texture, so passing the wrong one is a native crash rather than a build error. Inside an interop area a native handle is a pointer to its own opaque struct in Primitives, and genuinely untyped memory is void*. nuint is not covered by this rule: it is the correct mapping for size_t. A handle deliberately exposed on the public surface lives outside Interop and is not reported here.",
        Documentation + "khi002.md");

    internal static readonly DiagnosticDescriptor BooleanField = new(
        "KHI009",
        "bool in an interop struct",
        "{0} is declared as bool; in {1} a native boolean is stored as {2}",
        Interop,
        DiagnosticSeverity.Error,
        true,
        "bool is not blittable, so a struct that holds one requires marshalling: the LibraryImport generator refuses to pass it by value, and [MarshalAs] on the field does not change that. On a struct that is only reinterpreted through a pointer the attribute does nothing at all. Storing the field at its native width keeps the struct blittable and the layout honest.",
        Documentation + "khi009.md");
}
