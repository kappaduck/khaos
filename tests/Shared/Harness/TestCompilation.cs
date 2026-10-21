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

        CSharpCompilationOptions options = new(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, nullableContextOptions: NullableContextOptions.Enable);
        return CSharpCompilation.Create("KappaDuck.Khaos.Testing.Compilation", trees, References.All, options);
    }
}
