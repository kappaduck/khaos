// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Interop.SDL.Primitives;
using KappaDuck.Khaos.Mathematics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TUnit.Assertions.Enums;

namespace KappaDuck.Khaos.Tests.Interop.SDL;

internal sealed class LayoutTests
{
    private const int DisplayModeFieldsBeforeInternal = 32;

    [Test]
    public async Task ColorShouldHaveTheSizeOfSDLColor()
    {
        int size = Unsafe.SizeOf<Color>();

        await size.Should().BeEqualTo(4);
    }

    [Test]
    public async Task ColorShouldStoreItsRGBAInTheOrderOfSDLColor()
    {
        Color[] color = [new(1, 2, 3, 4)];

        byte[] expected = [1, 2, 3, 4];

        byte[] fields = unsafe (MemoryMarshal.Cast<Color, byte>(color)).ToArray();

        await fields.Should().BeEquivalentTo(expected, EqualityComparer<byte>.Default, CollectionOrdering.Matching);
    }

    [Test]
    public async Task DisplayModeShouldHaveTheSizeOfSDLDisplayMode()
    {
        int size = Unsafe.SizeOf<SDL_DisplayMode>();

        await size.Should().BeEqualTo(DisplayModeFieldsBeforeInternal + nint.Size);
    }

    [Test]
    public async Task DisplayModePixelDensityShouldSitAtTheOffsetOfSDLDisplayMode()
    {
        nint offset = Marshal.OffsetOf<SDL_DisplayMode>(nameof(SDL_DisplayMode.PixelDensity));

        await offset.Should().BeEqualTo(16);
    }

    [Test]
    public async Task DisplayModeRefreshRateShouldSitAtTheOffsetOfSDLDisplayMode()
    {
        nint offset = Marshal.OffsetOf<SDL_DisplayMode>(nameof(SDL_DisplayMode.RefreshRate));

        await offset.Should().BeEqualTo(20);
    }

    [Test]
    public async Task PointShouldHaveTheSizeOfSDLFPoint()
    {
        int size = Unsafe.SizeOf<Point>();

        await size.Should().BeEqualTo(8);
    }

    [Test]
    public async Task PointShouldStoreItsCoordinatesInTheOrderOfSDLFPoint()
    {
        Point[] point = [new(1, 2)];
        float[] expected = [1, 2];

        float[] fields = unsafe (MemoryMarshal.Cast<Point, float>(point)).ToArray();

        await fields.Should().BeEquivalentTo(expected, EqualityComparer<float>.Default, CollectionOrdering.Matching);
    }

    [Test]
    public async Task PointIShouldHaveTheSizeOfSDLPoint()
    {
        int size = Unsafe.SizeOf<PointI>();

        await size.Should().BeEqualTo(8);
    }

    [Test]
    public async Task PointIShouldStoreItsCoordinatesInTheOrderOfSDLPoint()
    {
        PointI[] point = [new(1, 2)];
        int[] expected = [1, 2];

        int[] fields = unsafe (MemoryMarshal.Cast<PointI, int>(point)).ToArray();

        await fields.Should().BeEquivalentTo(expected, EqualityComparer<int>.Default, CollectionOrdering.Matching);
    }

    [Test]
    public async Task RectShouldHaveTheSizeOfSDLFRect()
    {
        int size = Unsafe.SizeOf<Rect>();

        await size.Should().BeEqualTo(16);
    }

    [Test]
    public async Task RectShouldStoreItsFieldsInTheOrderOfSDLFRect()
    {
        Rect[] rect = [new(1, 2, 3, 4)];
        float[] expected = [1, 2, 3, 4];

        float[] fields = unsafe (MemoryMarshal.Cast<Rect, float>(rect)).ToArray();

        await fields.Should().BeEquivalentTo(expected, EqualityComparer<float>.Default, CollectionOrdering.Matching);
    }

    [Test]
    public async Task RectIShouldHaveTheSizeOfSDLRect()
    {
        int size = Unsafe.SizeOf<RectI>();

        await size.Should().BeEqualTo(16);
    }

    [Test]
    public async Task RectIShouldStoreItsFieldsInTheOrderOfSDLRect()
    {
        RectI[] rect = [new(1, 2, 3, 4)];
        int[] expected = [1, 2, 3, 4];

        int[] fields = unsafe (MemoryMarshal.Cast<RectI, int>(rect)).ToArray();

        await fields.Should().BeEquivalentTo(expected, EqualityComparer<int>.Default, CollectionOrdering.Matching);
    }

    [Test]
    public async Task SizeShouldHaveTheSizeOfTwoFloats()
    {
        int size = Unsafe.SizeOf<Size>();

        await size.Should().BeEqualTo(8);
    }

    [Test]
    public async Task SizeShouldStoreTheWidthBeforeTheHeight()
    {
        Size[] value = [new(1, 2)];
        float[] expected = [1, 2];

        float[] fields = unsafe (MemoryMarshal.Cast<Size, float>(value)).ToArray();

        await fields.Should().BeEquivalentTo(expected, EqualityComparer<float>.Default, CollectionOrdering.Matching);
    }

    [Test]
    public async Task SizeIShouldHaveTheSizeOfTwoInts()
    {
        int size = Unsafe.SizeOf<SizeI>();

        await size.Should().BeEqualTo(8);
    }

    [Test]
    public async Task SizeIShouldStoreTheWidthBeforeTheHeight()
    {
        SizeI[] value = [new(1, 2)];
        int[] expected = [1, 2];

        int[] fields = unsafe (MemoryMarshal.Cast<SizeI, int>(value)).ToArray();

        await fields.Should().BeEquivalentTo(expected, EqualityComparer<int>.Default, CollectionOrdering.Matching);
    }
}
