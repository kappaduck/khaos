// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Shared;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Analyzers.Tests.Shared;

internal sealed class EquatableArrayTests
{
    [Test]
    public async Task EqualsShouldTreatDefaultAsEmpty()
    {
        EquatableArray<string> uninitialized = default;

        bool result = uninitialized.Equals(EquatableArray<string>.Empty);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task EqualsShouldReturnTrueForAnArrayWithTheSameElements()
    {
        EquatableArray<string> left = ImmutableArray.Create("window", "renderer").ToEquatableArray();
        EquatableArray<string> right = ImmutableArray.Create("window", "renderer").ToEquatableArray();

        bool result = left.Equals(right);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task EqualsShouldReturnFalseForAnArrayWithADifferentElement()
    {
        EquatableArray<string> left = ImmutableArray.Create("left", "renderer").ToEquatableArray();
        EquatableArray<string> right = ImmutableArray.Create("right", "renderer").ToEquatableArray();

        bool result = left.Equals(right);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task EqualsShouldReturnFalseForAnArrayOfAnotherLength()
    {
        EquatableArray<string> left = ImmutableArray.Create("window", "renderer").ToEquatableArray();
        EquatableArray<string> right = ImmutableArray.Create("window").ToEquatableArray();

        bool result = left.Equals(right);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task EqualsShouldCompareRecordElementsByValue()
    {
        EquatableArray<Property> left = new[] { new Property("Width", "int") }.ToEquatableArray();
        EquatableArray<Property> right = new[] { new Property("Width", "int") }.ToEquatableArray();

        bool result = left.Equals(right);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task EqualsShouldLetARecordHoldingTheArrayCompareByValue()
    {
        Model first = new("Window", ImmutableArray.Create("Height", "Width").ToEquatableArray());
        Model second = new("Window", ImmutableArray.Create("Height", "Width").ToEquatableArray());

        bool result = first.Equals(second);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task EqualsObjectShouldReturnTrueForTheSameArrayBoxed()
    {
        EquatableArray<string> array = ImmutableArray.Create("window", "renderer").ToEquatableArray();
        object boxed = ImmutableArray.Create("window", "renderer").ToEquatableArray();

        bool result = array.Equals(boxed);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task EqualsObjectShouldReturnFalseForNull()
    {
        EquatableArray<string> array = ImmutableArray.Create("window", "renderer").ToEquatableArray();

        bool result = array.Equals(null);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task EqualsObjectShouldReturnFalseForAnotherType()
    {
        EquatableArray<string> array = ImmutableArray.Create("window", "renderer").ToEquatableArray();
        object immutable = ImmutableArray.Create("window", "renderer");

        bool result = array.Equals(immutable);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task GetHashCodeShouldMatchForArraysWithTheSameElements()
    {
        EquatableArray<string> left = ImmutableArray.Create("window", "renderer").ToEquatableArray();
        EquatableArray<string> right = ImmutableArray.Create("window", "renderer").ToEquatableArray();

        int hash = left.GetHashCode();

        await hash.Should().BeEqualTo(right.GetHashCode());
    }

    [Test]
    public async Task GetHashCodeShouldMatchForDefaultAndEmpty()
    {
        EquatableArray<string> uninitialized = default;

        int hash = uninitialized.GetHashCode();

        await hash.Should().BeEqualTo(EquatableArray<string>.Empty.GetHashCode());
    }


    [Test]
    public async Task GetHashCodeShouldMatchForArraysHoldingNull()
    {
        EquatableArray<string> left = ImmutableArray.Create(null!, "window").ToEquatableArray();
        EquatableArray<string> right = ImmutableArray.Create(null!, "window").ToEquatableArray();

        int hash = left.GetHashCode();

        await hash.Should().BeEqualTo(right.GetHashCode());
    }

    private sealed record Property(string Name, string Type);

    private sealed record Model(string Name, EquatableArray<string> Properties);
}
