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
}
