// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace KappaDuck.Khaos.Internal.Analyzers;

internal static class Descriptors
{
    private const string Documentation = "https://github.com/kappaduck/khaos/blob/main/docs/rules/";
    private const string Interop = "Interop";
    private const string InteropBoolDescription = "A native bool is 1 byte in SDL and a 4-byte BOOL in Win32, and nothing fails when the wrong width is marshalled: the value is simply wrong, sometimes. The area of the binding decides the width, written as UnmanagedType.U1 or UnmanagedType.Bool.";
    private const string InteropBoolStorageDescription = "A bool makes its struct non-blittable: passed by value, the struct needs runtime marshalling disabled, and read through a pointer, its managed layout no longer matches a 4-byte Win32 BOOL. Store a native boolean as byte in SDL and int in Win32, and convert to bool in the public wrapper.";
    private const string InteropNintDescription = "An nint compiles wherever any other handle or integer is expected, so passing the wrong one builds and fails later as a native crash. A typed pointer (SDL_Window*, or void* for untyped memory) or a struct that wraps a pointer-sized integer (LPARAM) turns the same mistake into a build error.";
    private const string InteropEntryPointDescription = "The managed name drops the library prefix, so nothing else in the declaration says which native symbol it binds. EntryPoint is that link, and it is written even when it matches the managed name: a rename would otherwise compile and break the binding.";
    private const string InteropVoidPointerDescription = "void* is right when the native side is genuinely untyped, and wrong when the real type was simply not looked up; the two look identical in the source. [UntypedBecause] carries the sentence that tells them apart, on the method where the decision was made.";
    private const string InteropPublicPointerDescription = "A game should never hold a pointer or a native handle that Khaos owns: the lifetime is not theirs, and nothing stops them from using it after the object behind it is gone. Keep it in a private field and expose what the caller needs; a native handle that must be handed out goes through a type that represents it, such as WindowHandle.";
    private const string InteropSDLCharDescription = "A C char is 1 byte and SDL strings are UTF-8, while a C# char is a 2-byte UTF-16 code unit. A char field shifts every field after it, and a char* reads two bytes per character. Store SDL text as byte and decode it as UTF-8.";
    private const string InteropLibraryDescription = "The other interop rules take their conventions from the area, so a binding in the wrong area is checked against the wrong ones. A library no area declares has not been decided on; add it to its area in InteropAreas first.";

    internal static readonly DiagnosticDescriptor InteropBoolReturn = new(
        "KHI001",
        "bool does not follow the marshalling convention of its interop area",
        "The return value of '{0}' does not follow the {1} bool convention; expected [return: MarshalAs(UnmanagedType.{2})]",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropBoolDescription,
        $"{Documentation}khi001.md");

    internal static readonly DiagnosticDescriptor InteropBoolParameter = new(
        "KHI001",
        "bool does not follow the marshalling convention of its interop area",
        "Parameter '{0}' of '{1}' does not follow the {2} bool convention; expected [MarshalAs(UnmanagedType.{3})]",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropBoolDescription,
        $"{Documentation}khi001.md");

    internal static readonly DiagnosticDescriptor InteropNintReturn = new(
        "KHI002",
        "nint or IntPtr in an interop declaration",
        "The return value of '{0}' uses nint; type it as the native pointer it carries, or wrap a pointer-sized integer in its own struct",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropNintDescription,
        $"{Documentation}khi002.md");

    internal static readonly DiagnosticDescriptor InteropNintParameter = new(
        "KHI002",
        "nint or IntPtr in an interop declaration",
        "Parameter '{0}' of '{1}' uses nint; type it as the native pointer it carries, or wrap a pointer-sized integer in its own struct",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropNintDescription,
        $"{Documentation}khi002.md");

    internal static readonly DiagnosticDescriptor InteropNintField = new(
        "KHI002",
        "nint or IntPtr in an interop declaration",
        "Field '{0}.{1}' uses nint; type it as the native pointer it carries, or wrap a pointer-sized integer in its own struct",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropNintDescription,
        $"{Documentation}khi002.md");

    internal static readonly DiagnosticDescriptor InteropNintProperty = new(
        "KHI002",
        "nint or IntPtr in an interop declaration",
        "Property '{0}.{1}' uses nint; type it as the native pointer it carries, or wrap a pointer-sized integer in its own struct",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropNintDescription,
        $"{Documentation}khi002.md");

    internal static readonly DiagnosticDescriptor InteropEntryPointMissing = new(
        "KHI003",
        "LibraryImport without an explicit EntryPoint",
        "'{0}' does not set EntryPoint; name the native symbol it binds",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropEntryPointDescription,
        $"{Documentation}khi003.md");

    internal static readonly DiagnosticDescriptor InteropEntryPointEmpty = new(
        "KHI003",
        "LibraryImport without an explicit EntryPoint",
        "'{0}' sets an empty EntryPoint; name the native symbol it binds",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropEntryPointDescription,
        $"{Documentation}khi003.md");

    internal static readonly DiagnosticDescriptor InteropVoidPointerUnexplained = new(
        "KHI004",
        "void* without a stated reason",
        "'{0}' takes or returns void* without [UntypedBecause]; say why it cannot be typed",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropVoidPointerDescription,
        $"{Documentation}khi004.md");

    internal static readonly DiagnosticDescriptor InteropVoidPointerEmptyReason = new(
        "KHI004",
        "void* without a stated reason",
        "'{0}' gives [UntypedBecause] an empty reason; say why it cannot be typed",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropVoidPointerDescription,
        $"{Documentation}khi004.md");

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

    internal static readonly DiagnosticDescriptor InteropBoolField = new(
        "KHI007",
        "bool in an interop struct",
        "Field '{0}.{1}' is declared as bool; in {2} a native boolean is stored as {3}",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropBoolStorageDescription,
        $"{Documentation}khi007.md");

    internal static readonly DiagnosticDescriptor InteropBoolProperty = new(
        "KHI007",
        "bool in an interop struct",
        "Property '{0}.{1}' is declared as bool; in {2} a native boolean is stored as {3}",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropBoolStorageDescription,
        $"{Documentation}khi007.md");

    internal static readonly DiagnosticDescriptor InteropPublicPointer = new(
        "KHI008",
        "raw pointer on the public surface",
        "'{0}' exposes a raw pointer; the pointer stays behind the wrapper",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropPublicPointerDescription,
        $"{Documentation}khi008.md");

    internal static readonly DiagnosticDescriptor InteropPublicNint = new(
        "KHI008",
        "raw pointer on the public surface",
        "'{0}' exposes nint; wrap the native handle in a type that represents it",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropPublicPointerDescription,
        $"{Documentation}khi008.md");

    internal static readonly DiagnosticDescriptor InteropImportOutsideArea = new(
        "KHI009",
        "native import outside an interop area",
        "'{0}' imports a native function outside Interop/SDL and Interop/Win32; move it to the area of its library",
        Interop,
        DiagnosticSeverity.Error,
        true,
        "Every rule on native bindings takes its conventions from the area a binding lives in. A [LibraryImport] outside Interop/SDL and Interop/Win32 has no area, so none of them checks it.",
        $"{Documentation}khi009.md");

    internal static readonly DiagnosticDescriptor InteropCallbackNotBlittable = new(
        "KHI010",
        "bool or char in a native callback",
        "'{0}' has {1} in a native callback signature; it is not blittable and Native AOT stops the process on it, use {2}",
        Interop,
        DiagnosticSeverity.Error,
        true,
        "A native callback is called without marshalling, so its signature must be blittable. bool and char compile, but Native AOT fails fast when native code calls such a method, and their native width depends on the side that calls.",
        $"{Documentation}khi010.md");

    internal static readonly DiagnosticDescriptor InteropSDLCharReturn = new(
        "KHI011",
        "char in Interop/SDL",
        "The return value of '{0}' uses char; a C char is 1 byte in SDL, store it as byte",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropSDLCharDescription,
        $"{Documentation}khi011.md");

    internal static readonly DiagnosticDescriptor InteropSDLCharParameter = new(
        "KHI011",
        "char in Interop/SDL",
        "Parameter '{0}' of '{1}' uses char; a C char is 1 byte in SDL, store it as byte",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropSDLCharDescription,
        $"{Documentation}khi011.md");

    internal static readonly DiagnosticDescriptor InteropSDLCharField = new(
        "KHI011",
        "char in Interop/SDL",
        "Field '{0}.{1}' uses char; a C char is 1 byte in SDL, store it as byte",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropSDLCharDescription,
        $"{Documentation}khi011.md");

    internal static readonly DiagnosticDescriptor InteropSDLCharProperty = new(
        "KHI011",
        "char in Interop/SDL",
        "Property '{0}.{1}' uses char; a C char is 1 byte in SDL, store it as byte",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropSDLCharDescription,
        $"{Documentation}khi011.md");
}
