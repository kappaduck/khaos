// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace KappaDuck.Khaos.Testing.Harness;

internal static class TestCompilation
{
    internal static CSharpCompilation Create(params string[] sources)
    {
        SyntaxTree[] trees = [.. sources.Select(static (source, index) => CSharpSyntaxTree.ParseText(source, Parsing.Preview, path: $"Source{index}.cs"))];

        return CSharpCompilation.Create(
            assemblyName: "KappaDuck.Khaos.Testing.Compilation",
            syntaxTrees: trees,
            references: References.All,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
    }
}
