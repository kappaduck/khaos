// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

internal static class References
{
    internal static ImmutableArray<MetadataReference> All { get; } = [.. Net110.References.All];
}
