// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;
using System.Globalization;

namespace KappaDuck.Khaos.Tests.Mathematics;

public sealed class SizeITests
{
    [Test]
    [Arguments(10, 5, false)]
    [Arguments(0, 5, true)]
    [Arguments(5, 0, true)]
    [Arguments(-1, 5, true)]
    public async Task IsEmptyShouldBeTrueForAZeroOrNegativeDimension(int width, int height, bool expected)
    {
        await new SizeI(width, height).IsEmpty.Should().BeEqualTo(expected);
    }

    [Test]
    public async Task AreaShouldNotOverflowForLargeSizes()
    {
        SizeI world = new(50_000, 50_000);

        await world.Area.Should().BeEqualTo(2_500_000_000L);
    }

    [Test]
    public async Task MultiplyingByAWholeFactorShouldStayInteger()
    {
        SizeI result = new SizeI(640, 360) * 2;

        await result.Should().BeEqualTo(new SizeI(1280, 720));
    }

    [Test]
    public async Task MultiplyingAWholeFactorBySizeShouldStayInteger()
    {
        SizeI result = 3 * new SizeI(640, 360);

        await result.Should().BeEqualTo(new SizeI(1920, 1080));
    }

    [Test]
    public async Task MultiplyingByAFractionalFactorShouldGiveASize()
    {
        Size result = new SizeI(640, 360) * 1.5f;

        await result.Should().BeEqualTo(new Size(960f, 540f));
    }

    [Test]
    public async Task ShouldConvertImplicitlyToSize()
    {
        Size size = new SizeI(1280, 720);

        await size.Should().BeEqualTo(new Size(1280f, 720f));
    }

    [Test]
    public async Task DeconstructShouldGiveBothDimensions()
    {
        (int width, int height) = new SizeI(1280, 720);

        await (width, height).Should().BeEqualTo((1280, 720));
    }

    [Test]
    public async Task ToStringShouldWriteBothDimensions()
    {
        string result = new SizeI(1280, 720).ToString(null, CultureInfo.InvariantCulture);

        await result.Should().BeEqualTo("(1280, 720)");
    }
}
