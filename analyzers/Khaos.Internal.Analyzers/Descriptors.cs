// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace KappaDuck.Khaos.Internal.Analyzers;

internal static class Descriptors
{
    private const string Documentation = "https://github.com/kappaduck/khaos/blob/main/docs/rules/";
    private const string Interop = "Interop";

    internal static readonly DiagnosticDescriptor InteropVisibility = new(
        "KHI005",
        "type in Interop is not internal",
        "'{0}' is visible outside the assembly; every type in Interop is internal",
        Interop,
        DiagnosticSeverity.Error,
        true,
        "Once every interop type is internal, the compiler refuses any public signature that mentions one, so the boundary holds without walking signatures. A type that has to be seen from outside does not belong in Interop.",
        $"{Documentation}khi005.md");
}
