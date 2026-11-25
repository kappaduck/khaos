// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace KappaDuck.Khaos.Analyzers.Tests.Helpers;

internal static class KhaosFramework
{
    private static readonly MetadataReference[] _references =
    [
        .. Directory.EnumerateFiles(AppContext.BaseDirectory, "KappaDuck.Khaos.dll")
            .Concat(Directory.EnumerateFiles(AppContext.BaseDirectory, "Microsoft.Extensions.*.dll"))
            .Select(static p => MetadataReference.CreateFromFile(p))
    ];

    internal static CSharpCompilation Compile(params ReadOnlySpan<string> sources)
    {
        return TestCompilation.Create(Parsing.Framework, sources)
                              .AddReferences(_references);
    }
}
