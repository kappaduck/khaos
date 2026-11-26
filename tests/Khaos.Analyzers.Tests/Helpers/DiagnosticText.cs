// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace KappaDuck.Khaos.Analyzers.Tests.Helpers;

internal static class DiagnosticText
{
    internal static string Ids(IEnumerable<Diagnostic> diagnostics)
        => string.Join(", ", diagnostics.Select(static diagnostic => diagnostic.Id).Order(StringComparer.Ordinal));

    internal static string Join(IEnumerable<Diagnostic> diagnostics)
        => string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString()));

    internal static string Locate(string id, string path, LinePositionSpan span)
    => $"{id} {path}({span.Start.Line + 1},{span.Start.Character + 1})-({span.End.Line + 1},{span.End.Character + 1})";

    internal static string Located(IEnumerable<Diagnostic> diagnostics)
        => string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => Locate(diagnostic)));

    internal static IOrderedEnumerable<Diagnostic> InSourceOrder(IEnumerable<Diagnostic> diagnostics)
    {
        return diagnostics
            .OrderBy(static d => d.Location.SourceTree?.FilePath, StringComparer.Ordinal)
            .ThenBy(static d => d.Location.SourceSpan.Start)
            .ThenBy(static d => d.Id, StringComparer.Ordinal);
    }

    private static string Locate(Diagnostic diagnostic)
    {
        FileLinePositionSpan span = diagnostic.Location.GetLineSpan();
        return Locate(diagnostic.Id, span.Path, span.Span);
    }
}
