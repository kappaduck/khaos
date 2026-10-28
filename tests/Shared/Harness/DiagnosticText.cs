// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace KappaDuck.Khaos.Testing.Harness;

internal static class DiagnosticText
{
    internal static string Ids(IEnumerable<Diagnostic> diagnostics)
        => string.Join(", ", diagnostics.Select(static diagnostic => diagnostic.Id).Order(StringComparer.Ordinal));

    internal static string Join(IEnumerable<Diagnostic> diagnostics)
        => string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString()));

    internal static IOrderedEnumerable<Diagnostic> InSourceOrder(IEnumerable<Diagnostic> diagnostics)
    {
        return diagnostics
            .OrderBy(static d => d.Location.SourceTree?.FilePath, StringComparer.Ordinal)
            .ThenBy(static d => d.Location.SourceSpan.Start)
            .ThenBy(static d => d.Id, StringComparer.Ordinal);
    }
}
