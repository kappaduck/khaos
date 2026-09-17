// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis.CSharp;

namespace KappaDuck.Khaos.Testing.Harness;

internal static class Parsing
{
    internal static readonly CSharpParseOptions Preview = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
}
