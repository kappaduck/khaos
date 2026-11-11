// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace KappaDuck.Khaos.Internal.Analyzers;

internal static class Descriptors
{
    private const string Documentation = "https://github.com/kappaduck/khaos/blob/main/docs/rules/";
    private const string EventCaseConstructorDescription = "The generated converter builds a case from the native event SDL hands to the game. It finds the field of SDL_Event to read by the type of the constructor's first parameter, and passes the EventContext when the constructor asks for it. A case without that constructor cannot be built, so the generator leaves it out of Event.";
    private const string EventCaseDuplicateDescription = "One native event becomes one case: the converter switches on the native type, and a type claimed twice would make one of the cases unreachable. The generator keeps the case that sorts first and leaves the other out of Event.";
    private const string EventCaseFieldDescription = "Event overlaps its unmanaged cases in the same bytes, and the runtime refuses a reference that overlaps anything else. A string gets a field of its own; any other reference would also make a case that a game can keep alive by mistake.";
    private const string EventCaseShapeDescription = "Every case is copied into Event and handed to OnEvent by in, so it has to be an immutable value the game can read and match: a public, non-generic, top-level readonly record struct.";
    private const string Events = "Events";
    private const string Interop = "Interop";
    private const string InteropBoolDescription = "A native bool is 1 byte in SDL and a 4-byte BOOL in Win32, and nothing fails when the wrong width is marshalled: the value is simply wrong, sometimes. The area of the binding decides the width, written as UnmanagedType.U1 or UnmanagedType.Bool.";
    private const string InteropBoolStorageDescription = "A bool makes its struct non-blittable: passed by value, the struct needs runtime marshalling disabled, and read through a pointer, its managed layout no longer matches a 4-byte Win32 BOOL. Store a native boolean as byte in SDL and int in Win32, and convert to bool in the public wrapper.";
    private const string InteropEntryPointDescription = "The managed name drops the library prefix, so nothing else in the declaration says which native symbol it binds. EntryPoint is that link, and it is written even when it matches the managed name: a rename would otherwise compile and break the binding.";
    private const string InteropLibraryDescription = "The other interop rules take their conventions from the area, so a binding in the wrong area is checked against the wrong ones. A library no area declares has not been decided on; add it to its area in InteropAreas first.";
    private const string InteropNintDescription = "An nint compiles wherever any other handle or integer is expected, so passing the wrong one builds and fails later as a native crash. A typed pointer (SDL_Window*, or void* for untyped memory) or a struct that wraps a pointer-sized integer (LPARAM) turns the same mistake into a build error.";
    private const string InteropNonCopyableDescription = "A [NonCopyable] value owns native memory and frees it when disposed. A second copy frees it a second time, a value no using owns is never freed, and a value disposed by hand is freed again by its using. Its using is the one owner: read it through its members, pass it by in, or return it to a caller who gives it its own using.";
    private const string InteropPublicPointerDescription = "A game should never hold a pointer or a native handle that Khaos owns: the lifetime is not theirs, and nothing stops them from using it after the object behind it is gone. Keep it in a private field and expose what the caller needs; a native handle that must be handed out goes through a type that represents it, such as WindowHandle.";
    private const string InteropSDLCharDescription = "A C char is 1 byte and SDL strings are UTF-8, while a C# char is a 2-byte UTF-16 code unit. A char field shifts every field after it, and a char* reads two bytes per character. Store SDL text as byte and decode it as UTF-8.";
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

    internal static readonly DiagnosticDescriptor InteropNonCopyableOutsideUsing = new(
        "KHI012",
        "non-copyable value used outside its using",
        "This '{0}' has no using to own it; declare it with using, or return it to the caller",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropNonCopyableDescription,
        $"{Documentation}khi012.md");

    internal static readonly DiagnosticDescriptor InteropNonCopyableCopied = new(
        "KHI012",
        "non-copyable value used outside its using",
        "'{0}' is copied; read it through its members, or pass it by in",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropNonCopyableDescription,
        $"{Documentation}khi012.md");

    internal static readonly DiagnosticDescriptor InteropNonCopyableDisposedByHand = new(
        "KHI012",
        "non-copyable value used outside its using",
        "'{0}' is disposed by hand; its using already disposes it",
        Interop,
        DiagnosticSeverity.Error,
        true,
        InteropNonCopyableDescription,
        $"{Documentation}khi012.md");

    internal static readonly DiagnosticDescriptor EventCaseShape = new(
        "KHI013",
        "event case is not a public readonly record struct",
        "'{0}' is marked [KhaosEvent]; declare it as a public, non-generic, top-level readonly record struct",
        Events,
        DiagnosticSeverity.Error,
        true,
        EventCaseShapeDescription,
        $"{Documentation}khi013.md");

    internal static readonly DiagnosticDescriptor EventCaseRange = new(
        "KHI013",
        "event case is not a public readonly record struct",
        "'{0}' covers a range of native types whose last type comes before its first",
        Events,
        DiagnosticSeverity.Error,
        true,
        EventCaseShapeDescription,
        $"{Documentation}khi013.md");

    internal static readonly DiagnosticDescriptor EventCaseField = new(
        "KHI014",
        "event case holds a reference other than a string",
        "'{0}' holds '{1}' of type {2}; an event case holds only unmanaged values and strings",
        Events,
        DiagnosticSeverity.Error,
        true,
        EventCaseFieldDescription,
        $"{Documentation}khi014.md");

    internal static readonly DiagnosticDescriptor EventCaseConstructor = new(
        "KHI015",
        "event case has no valid native constructor",
        "'{0}' {1}",
        Events,
        DiagnosticSeverity.Error,
        true,
        EventCaseConstructorDescription,
        $"{Documentation}khi015.md");

    internal static readonly DiagnosticDescriptor EventCaseDuplicate = new(
        "KHI016",
        "native event type claimed by two cases",
        "'{0}' claims a native event type that '{1}' already converts",
        Events,
        DiagnosticSeverity.Error,
        true,
        EventCaseDuplicateDescription,
        $"{Documentation}khi016.md",
        WellKnownDiagnosticTags.CompilationEnd);
}
