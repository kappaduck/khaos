// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis.CSharp;

namespace KappaDuck.Khaos.Analyzers.Tests.Helpers;

internal static class Parsing
{
    internal static readonly CSharpParseOptions Framework = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp15);

    internal static readonly CSharpParseOptions Preview = CSharpParseOptions.Default
        .WithLanguageVersion(LanguageVersion.Preview)
        .WithFeatures([new("updated-memory-safety-rules", "true")]);

    internal static readonly CSharpParseOptions WithoutMemorySafety = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
}
