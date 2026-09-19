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

    internal static readonly DiagnosticDescriptor EntryPoint = new(
        "KHI003",
        "LibraryImport without an explicit EntryPoint",
        "'{0}' does not set EntryPoint; name the native symbol it binds",
        Interop,
        DiagnosticSeverity.Error,
        true,
        "The managed name drops the library prefix, so nothing in the declaration says which native symbol is being bound. EntryPoint is that link, and it is what makes a binding searchable against the wiki page it came from. It is written even when it would match the managed name, because a name that happens to match today stops matching the moment the method is renamed.",
        Documentation + "khi003.md");

    internal static readonly DiagnosticDescriptor UntypedPointer = new(
        "KHI004",
        "void* without a stated reason",
        "'{0}' takes or returns void* without [UntypedPointer]; say why it cannot be typed",
        Interop,
        DiagnosticSeverity.Error,
        true,
        "void* is the fallback when the native side is genuinely untyped, not when the real type is unknown. Requiring a written reason on the declaration separates the two: a userdata pointer SDL hands back untouched has one, a handle nobody looked up does not. The reason is an attribute rather than a comment so that it survives a reformat and can be found by searching.",
        Documentation + "khi004.md");

    internal static readonly DiagnosticDescriptor InteropVisibility = new(
        "KHI005",
        "type in an interop area is not internal",
        "'{0}' is visible outside the assembly; every type in an interop area is internal",
        Interop,
        DiagnosticSeverity.Error,
        true,
        "Once every interop type is internal the compiler enforces the rest of the boundary by itself: a public signature that mentions one fails with an accessibility error before any analyzer runs. That is why this rule guards the declaration rather than the signatures. A native handle deliberately exposed to third-party interop lives outside Interop and is not reported here.",
        Documentation + "khi005.md");

    internal static readonly DiagnosticDescriptor NativeLibrary = new(
        "KHI006",
        "native library does not match its interop area",
        "'{0}' does not belong to {1}; {2}",
        Interop,
        DiagnosticSeverity.Error,
        true,
        "The interop area decides which conventions KHI001 and KHI002 apply, so a binding that sits in the wrong area gets the wrong one applied to it in silence: a Win32 BOOL read one byte at a time, on a green build. A library that no area declares is reported for the same reason, which makes adding a native dependency a deliberate edit of InteropAreas rather than a binding that lands wherever a file happened to be open.",
        Documentation + "khi006.md");

    internal static readonly DiagnosticDescriptor PublicPointer = new(
        "KHI008",
        "raw pointer on the public surface",
        "'{0}' exposes a raw pointer; the pointer stays behind the wrapper",
        Interop,
        DiagnosticSeverity.Error,
        true,
        "KHI005 keeps interop types internal, which the compiler then enforces across every public signature. Two shapes slip through it: void* and a function pointer, whose parts are all public types. This rule closes them. nint is not a pointer and is not reported, which is what lets a native handle be exposed on purpose.",
        Documentation + "khi008.md");

    internal static readonly DiagnosticDescriptor BooleanField = new(
        "KHI007",
        "bool in an interop struct",
        "{0} is declared as bool; in {1} a native boolean is stored as {2}",
        Interop,
        DiagnosticSeverity.Error,
        true,
        "bool is not blittable, so a struct that holds one requires marshalling: the LibraryImport generator refuses to pass it by value, and [MarshalAs] on the field does not change that. On a struct that is only reinterpreted through a pointer the attribute does nothing at all. Storing the field at its native width keeps the struct blittable and the layout honest.",
        Documentation + "khi007.md");
}
