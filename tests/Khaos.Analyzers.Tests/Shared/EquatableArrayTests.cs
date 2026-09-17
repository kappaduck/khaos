// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Shared;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Analyzers.Tests.Shared;

internal sealed class EquatableArrayTests
{
    [Test]
    public async Task ArraysWithTheSameContentShouldBeEqual()
    {
        EquatableArray<string> first = ImmutableArray.Create("window", "renderer").ToEquatableArray();
        EquatableArray<string> second = ImmutableArray.Create("window", "renderer").ToEquatableArray();

        bool result = first == second;

        await result.Should().BeTrue();
        await first.GetHashCode().Should().BeEqualTo(second.GetHashCode());
    }

    [Test]
    public async Task ImmutableArrayShouldStillCompareByReference()
    {
        ImmutableArray<string> first = ["window", "renderer"];
        ImmutableArray<string> second = ["window", "renderer"];

        bool result = first.Equals(second);
        await result.Should().BeFalse();
    }

    [Test]
    public async Task DefaultShouldBeEqualToEmpty()
    {
        EquatableArray<string> uninitialized = default;

        bool result = uninitialized == EquatableArray<string>.Empty;

        await result.Should().BeTrue();
        await uninitialized.GetHashCode().Should().BeEqualTo(EquatableArray<string>.Empty.GetHashCode());
        await uninitialized.Length.Should().BeEqualTo(0);
    }

    [Test]
    public async Task ArraysWithADifferentOrderShouldNotBeEqual()
    {
        EquatableArray<string> first = ImmutableArray.Create("window", "renderer").ToEquatableArray();
        EquatableArray<string> second = ImmutableArray.Create("renderer", "window").ToEquatableArray();

        bool result = first != second;
        await result.Should().BeTrue();
    }

    [Test]
    public async Task ArraysWithADifferentLengthShouldNotBeEqual()
    {
        EquatableArray<string> first = ImmutableArray.Create("window", "renderer").ToEquatableArray();
        EquatableArray<string> second = ImmutableArray.Create("window").ToEquatableArray();

        bool result = first == second;
        await result.Should().BeFalse();
    }

    [Test]
    public async Task RecordElementsShouldCompareStructurally()
    {
        EquatableArray<Property> first = new[] { new Property("Width", "int") }.ToEquatableArray();
        EquatableArray<Property> second = new[] { new Property("Width", "int") }.ToEquatableArray();
        EquatableArray<Property> third = new[] { new Property("Width", "float") }.ToEquatableArray();

        await (first == second).Should().BeTrue();
        await (first == third).Should().BeFalse();
    }

    [Test]
    public async Task ModelsHoldingAnEquatableArrayShouldCompareByValue()
    {
        Model first = new("Window", ImmutableArray.Create("Height", "Width").ToEquatableArray());
        Model second = new("Window", ImmutableArray.Create("Height", "Width").ToEquatableArray());

        bool result = first.Equals(second);
        await result.Should().BeTrue();
    }

    [Test]
    public async Task ForeachShouldEnumerateInOrder()
    {
        EquatableArray<string> array = ImmutableArray.Create("window", "renderer", "texture").ToEquatableArray();

        List<string> visited = [];
        visited.AddRange(array);

        await string.Join(",", visited).Should().BeEqualTo("window,renderer,texture");
    }

    [Test]
    public async Task DefaultShouldEnumerateWithoutThrowing()
    {
        EquatableArray<string> uninitialized = default;

        List<string> visited = [];
        visited.AddRange(uninitialized);

        await visited.Should().BeEmpty();
    }

    [Test]
    public async Task ImplicitConversionShouldWrapTheArray()
    {
        EquatableArray<string> array = ImmutableArray.Create("window", "renderer");

        await array.Length.Should().BeEqualTo(2);
        await array[1].Should().BeEqualTo("renderer");
    }

    private sealed record Property(string Name, string Type);

    private sealed record Model(string Name, EquatableArray<string> Properties);
}
