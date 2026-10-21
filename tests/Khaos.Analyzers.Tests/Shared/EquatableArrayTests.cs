// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Shared;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Analyzers.Tests.Shared;

internal sealed class EquatableArrayTests
{
    [Test]
    public async Task DefaultShouldBeEqualToEmpty()
    {
        EquatableArray<string> uninitialized = default;

        bool result = uninitialized.Equals(EquatableArray<string>.Empty);
        await result.Should().BeTrue();
    }

    [Test]
    public async Task ArraysWithTheSameContentShouldBeEqual()
    {
        EquatableArray<string> left = ImmutableArray.Create("window", "renderer").ToEquatableArray();
        EquatableArray<string> right = ImmutableArray.Create("window", "renderer").ToEquatableArray();

        bool result = left.Equals(right);
        await result.Should().BeTrue();
    }

    [Test]
    public async Task ArraysWithNotTheSameContentShouldBeNotEqual()
    {
        EquatableArray<string> left = ImmutableArray.Create("left", "renderer").ToEquatableArray();
        EquatableArray<string> right = ImmutableArray.Create("right", "renderer").ToEquatableArray();

        bool result = left.Equals(right);
        await result.Should().BeFalse();
    }

    [Test]
    public async Task ArraysWithNotSamelengthShouldBeNotEquals()
    {
        EquatableArray<string> left = ImmutableArray.Create("window", "renderer").ToEquatableArray();
        EquatableArray<string> right = ImmutableArray.Create("window").ToEquatableArray();

        bool result = left.Equals(right);
        await result.Should().BeFalse();
    }

    [Test]
    public async Task RecordElementsShouldCompareStructurally()
    {
        EquatableArray<Property> left = new[] { new Property("Width", "int") }.ToEquatableArray();
        EquatableArray<Property> right = new[] { new Property("Width", "int") }.ToEquatableArray();

        bool result = left.Equals(right);
        await result.Should().BeTrue();
    }

    [Test]
    public async Task ModelsHoldingAnEquatableArrayShouldCompareByValue()
    {
        Model first = new("Window", ImmutableArray.Create("Height", "Width").ToEquatableArray());
        Model second = new("Window", ImmutableArray.Create("Height", "Width").ToEquatableArray());

        bool result = first.Equals(second);
        await result.Should().BeTrue();
    }

    private sealed record Property(string Name, string Type);

    private sealed record Model(string Name, EquatableArray<string> Properties);
}
