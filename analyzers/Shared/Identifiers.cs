// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis.CSharp;

namespace KappaDuck.Khaos.Analyzers.Shared;

internal static class Identifiers
{
    internal static string Escape(string name) => SyntaxFacts.GetKeywordKind(name) == SyntaxKind.None ? name : "@" + name;
}
