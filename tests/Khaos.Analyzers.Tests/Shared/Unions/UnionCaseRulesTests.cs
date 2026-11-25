// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Shared.Unions;
using Microsoft.CodeAnalysis;

namespace KappaDuck.Khaos.Analyzers.Tests.Shared.Unions;

internal sealed class UnionCaseRulesTests
{
    [Test]
    public async Task FindReferenceFieldShouldReturnNullForAnUnmanagedType()
    {
        INamedTypeSymbol type = TypeOf("public readonly record struct Resized(int Width, int Height);", "Resized");

        IFieldSymbol? field = UnionCaseRules.FindReferenceField(type);

        await field.Should().BeNull();
    }

    [Test]
    public async Task FindReferenceFieldShouldReturnNullWhenTheOnlyReferenceIsAString()
    {
        INamedTypeSymbol type = TypeOf("public readonly record struct Dropped(string Path, int Count);", "Dropped");

        IFieldSymbol? field = UnionCaseRules.FindReferenceField(type);

        await field.Should().BeNull();
    }

    [Test]
    public async Task FindReferenceFieldShouldReturnTheFieldThatHoldsAnObject()
    {
        const string source = """
            public readonly struct Holder
            {
                public readonly int Count;
                public readonly object Payload;
            }
            """;

        string? name = UnionCaseRules.FindReferenceField(TypeOf(source, "Holder"))?.Name;

        await name.Should().BeEqualTo("Payload");
    }

    [Test]
    public async Task FindReferenceFieldShouldSkipStaticAndConstFields()
    {
        const string source = """
            public readonly struct Level
            {
                public static readonly object Shared = new();
                public const string Forest = "Forest";
                public readonly string Title;
            }
            """;

        IFieldSymbol? field = UnionCaseRules.FindReferenceField(TypeOf(source, "Level"));

        await field.Should().BeNull();
    }

    [Test]
    public async Task FindReferenceFieldShouldFindTheFieldBehindAProperty()
    {
        INamedTypeSymbol type = TypeOf("public readonly record struct Loaded(object Asset);", "Loaded");

        string? property = UnionCaseRules.FindReferenceField(type)?.AssociatedSymbol?.Name;

        await property.Should().BeEqualTo("Asset");
    }

    private static INamedTypeSymbol TypeOf(string source, string name) => TestCompilation.Create(source).GetTypeByMetadataName(name)!;
}
