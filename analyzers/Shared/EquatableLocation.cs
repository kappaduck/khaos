// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics.CodeAnalysis;

namespace KappaDuck.Khaos.Analyzers.Shared;

/// <summary>
/// A value-equatable stand-in for <see cref="Location"/>, safe to keep inside an incremental generator model.
/// </summary>
/// <remarks>
/// A source <see cref="Location"/> holds on to its <see cref="SyntaxTree"/>, so keeping one in a model roots
/// the whole compilation and forces Roslyn to hold on to lots of memory. Its equality is tied to the tree
/// instance as well, so any edit to any file invalidates it. This record keeps only the three values needed
/// to rebuild the location when a diagnostic is finally reported.
/// </remarks>
/// <param name="FilePath">The path of the source file the location points at.</param>
/// <param name="Text">The span within the source text.</param>
/// <param name="Line">The line and character span within the source text.</param>
internal sealed record EquatableLocation(string FilePath, TextSpan Text, LinePositionSpan Line)
{
    internal static EquatableLocation? From(Location? location)
    {
        if (location?.SourceTree is null)
            return null;

        return new EquatableLocation(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
    }

    [return: NotNullIfNotNull(nameof(node))]
    internal static EquatableLocation? From(SyntaxNode? node)
    {
        if (node is null)
            return null;

        return From(node.GetLocation())!;
    }

    [return: NotNullIfNotNull(nameof(reference))]
    internal static EquatableLocation? From(SyntaxReference? reference)
        => reference is null ? null : From(Location.Create(reference.SyntaxTree, reference.Span));

    internal static EquatableLocation? From(ISymbol? symbol)
        => From(symbol?.Locations.FirstOrDefault(static location => location.IsInSource));

    internal Location ToLocation() => Location.Create(FilePath, Text, Line);

    internal Diagnostic ToDiagnostic(DiagnosticDescriptor descriptor, params object?[] arguments)
        => Diagnostic.Create(descriptor, ToLocation(), arguments);
}
