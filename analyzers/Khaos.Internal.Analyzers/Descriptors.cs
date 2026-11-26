// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace KappaDuck.Khaos.Internal.Analyzers;

internal static class Descriptors
{
    private const string Documentation = "https://github.com/kappaduck/khaos/blob/main/docs/rules/";
    private const string Interop = "Interop";

    private const string InteropBoolDescription = "A native bool is 1 byte in SDL and a 4-byte BOOL in Win32, and nothing fails when the wrong width is marshalled: the value is simply wrong, sometimes. The area of the binding decides the width, written as UnmanagedType.U1 or UnmanagedType.Bool.";
    private const string InteropNintDescription = "An nint compiles wherever any other handle or integer is expected, so passing the wrong one builds and fails later as a native crash. A typed pointer (SDL_Window*, or void* for untyped memory) or a struct that wraps a pointer-sized integer (LPARAM) turns the same mistake into a build error.";
    private const string InteropEntryPointDescription = "The managed name drops the library prefix, so nothing else in the declaration says which native symbol it binds. EntryPoint is that link, and it is written even when it matches the managed name: a rename would otherwise compile and break the binding.";
    private const string InteropVoidPointerDescription = "void* is right when the native side is genuinely untyped, and wrong when the real type was simply not looked up; the two look identical in the source. [UntypedBecause] carries the sentence that tells them apart, on the method where the decision was made.";

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
}
