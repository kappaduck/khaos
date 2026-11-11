// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Testing.Harness;

internal static class TestCompilation
{
    internal static CSharpCompilation Create(params ReadOnlySpan<string> sources) => Compile(Parsing.Preview, sources, Net110.References.All);

    internal static CSharpCompilation Create(CSharpParseOptions parsing, params ReadOnlySpan<string> sources) => Compile(parsing, sources, Net110.References.All);

    internal static CSharpCompilation WithoutReferences(params ReadOnlySpan<string> sources) => Compile(Parsing.Preview, sources, []);

    internal static CSharpCompilation Edit(CSharpCompilation compilation, params ReadOnlySpan<string> edited)
    {
        ImmutableArray<SyntaxTree> trees = compilation.SyntaxTrees;

        if (edited.Length != trees.Length)
            throw new ArgumentException($"An edit changes the text of the {trees.Length} sources; it never adds or removes one.", nameof(edited));

        CSharpCompilation result = compilation;

        for (int i = 0; i < edited.Length; i++)
        {
            if (!string.Equals(trees[i].ToString(), edited[i], StringComparison.Ordinal))
                result = result.ReplaceSyntaxTree(trees[i], trees[i].WithChangedText(SourceText.From(edited[i])));
        }

        return result;
    }

    private static CSharpCompilation Compile(CSharpParseOptions parsing, ReadOnlySpan<string> sources, IEnumerable<MetadataReference> references)
    {
        SyntaxTree[] trees = new SyntaxTree[sources.Length];

        for (int i = 0; i < sources.Length; i++)
            trees[i] = CSharpSyntaxTree.ParseText(sources[i], parsing, $"Source{i}.cs");

        CSharpCompilationOptions options = new(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, nullableContextOptions: NullableContextOptions.Enable);
        return CSharpCompilation.Create("KappaDuck.Khaos.Testing.Compilation", trees, references, options);
    }
}
