// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;
using KappaDuck.Khaos.Windowing;

namespace KappaDuck.Khaos.Tests.Windowing;

internal sealed class WindowScaleTests
{
    [Test]
    public async Task ContentScaleShouldBeTheDisplayScaleWhenTheSizeIsInPixels()
    {
        float scale = WindowScale.ContentScale(displayScale: 2f, nativeWidth: 2560, pixelWidth: 2560);

        await scale.Should().BeEqualTo(2f);
    }

    [Test]
    public async Task ContentScaleShouldBeOneWhenTheSizeIsAlreadyLogical()
    {
        float scale = WindowScale.ContentScale(displayScale: 2f, nativeWidth: 1280, pixelWidth: 2560);

        await scale.Should().BeEqualTo(1f);
    }

    [Test]
    public async Task ContentScaleShouldFallBackToTheDisplayScaleWithoutPixels()
    {
        float scale = WindowScale.ContentScale(displayScale: 1.5f, nativeWidth: 1280, pixelWidth: 0);

        await scale.Should().BeEqualTo(1.5f);
    }

    [Test]
    public async Task ToNativeShouldMultiplyByTheContentScale()
    {
        SizeI logical = new(1280, 720);

        SizeI native = WindowScale.ToNative(logical, contentScale: 1.5f);

        await native.Should().BeEqualTo(new SizeI(1920, 1080));
    }

    [Test]
    public async Task ToLogicalShouldDivideByTheContentScale()
    {
        SizeI native = new(2560, 1440);

        SizeI logical = WindowScale.ToLogical(native, contentScale: 2f);

        await logical.Should().BeEqualTo(new SizeI(1280, 720));
    }

    [Test]
    public async Task ToLogicalShouldRoundToTheNearestUnit()
    {
        SizeI native = new(1001, 1000);

        SizeI logical = WindowScale.ToLogical(native, contentScale: 1.25f);

        await logical.Should().BeEqualTo(new SizeI(801, 800));
    }

    [Test]
    public async Task ToNativeShouldKeepZeroAsNoLimit()
    {
        SizeI native = WindowScale.ToNative(SizeI.Zero, contentScale: 2f);

        await native.Should().BeEqualTo(SizeI.Zero);
    }

    [Test]
    public async Task ToNativeShouldMultiplyAPointByTheContentScale()
    {
        PointI logical = new(100, -40);

        PointI native = WindowScale.ToNative(logical, contentScale: 1.5f);

        await native.Should().BeEqualTo(new PointI(150, -60));
    }

    [Test]
    public async Task ToLogicalShouldDivideAPointByTheContentScale()
    {
        PointI native = new(2560, 30);

        PointI logical = WindowScale.ToLogical(native, contentScale: 1.5f);

        await logical.Should().BeEqualTo(new PointI(1707, 20));
    }

    [Test]
    public async Task ToLogicalShouldDivideARectangleByTheContentScale()
    {
        RectI native = new(0, 60, 2560, 1380);

        RectI logical = WindowScale.ToLogical(native, contentScale: 2f);

        await logical.Should().BeEqualTo(new RectI(0, 30, 1280, 690));
    }

    [Test]
    public async Task ToPixelsShouldMultiplyTheSizeByThePixelDensity()
    {
        SizeI native = new(1280, 720);

        SizeI pixels = WindowScale.ToPixels(native, pixelDensity: 2f);

        await pixels.Should().BeEqualTo(new SizeI(2560, 1440));
    }

    [Test]
    public async Task ToPixelsShouldKeepTheSizeWhenItIsAlreadyInPixels()
    {
        SizeI native = new(2560, 1440);

        SizeI pixels = WindowScale.ToPixels(native, pixelDensity: 1f);

        await pixels.Should().BeEqualTo(native);
    }

    [Test]
    public async Task ToPixelsShouldMultiplyARectangleByThePixelDensity()
    {
        RectI native = new(0, 30, 1280, 690);

        RectI pixels = WindowScale.ToPixels(native, pixelDensity: 2f);

        await pixels.Should().BeEqualTo(new RectI(0, 60, 2560, 1380));
    }
}
