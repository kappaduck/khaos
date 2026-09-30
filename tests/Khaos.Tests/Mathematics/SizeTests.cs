// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;
using System.Globalization;
using System.Numerics;

namespace KappaDuck.Khaos.Tests.Mathematics;

public sealed class SizeTests
{
    [Test]
    [Arguments(10f, 5f, false)]
    [Arguments(0f, 5f, false)]
    [Arguments(0f, 0f, false)]
    [Arguments(-1f, 5f, true)]
    [Arguments(5f, -1f, true)]
    public async Task IsEmptyShouldBeTrueOnlyForANegativeDimension(float width, float height, bool expected)
    {
        await new Size(width, height).IsEmpty.Should().BeEqualTo(expected);
    }

    [Test]
    public async Task AreaShouldMultiplyTheDimensions()
    {
        await new Size(2.5f, 4f).Area.Should().BeEqualTo(10f);
    }

    [Test]
    public async Task CeilingShouldGiveASizeTheContentFitsIn()
    {
        SizeI texture = new Size(120.2f, 18.7f).Ceiling();

        await texture.Should().BeEqualTo(new SizeI(121, 19));
    }

    [Test]
    public async Task FloorShouldRoundEachDimensionDown()
    {
        await new Size(7.5f, 2.6f).Floor().Should().BeEqualTo(new SizeI(7, 2));
    }

    [Test]
    public async Task RoundShouldSendHalvesAwayFromZero()
    {
        await new Size(7.5f, 2.4f).Round().Should().BeEqualTo(new SizeI(8, 2));
    }

    [Test]
    public async Task TruncateShouldRoundEachDimensionTowardZero()
    {
        await new Size(7.9f, 2.6f).Truncate().Should().BeEqualTo(new SizeI(7, 2));
    }

    [Test]
    public async Task MultiplyingByAScalarShouldScaleBothDimensions()
    {
        Size result = new Size(16f, 24f) * 2f;

        await result.Should().BeEqualTo(new Size(32f, 48f));
    }

    [Test]
    public async Task MultiplyingAScalarByASizeShouldScaleBothDimensions()
    {
        Size result = 0.5f * new Size(16f, 24f);

        await result.Should().BeEqualTo(new Size(8f, 12f));
    }

    [Test]
    public async Task DivisionShouldDivideBothDimensions()
    {
        Size result = new Size(16f, 24f) / 4f;

        await result.Should().BeEqualTo(new Size(4f, 6f));
    }

    [Test]
    public async Task EqualityShouldAcceptIdenticalSizes()
    {
        Size size = new(1f, 1f);

        bool result = size == new Size(1f, 1f);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task EqualityShouldRejectSizesThatDifferByTheSmallestAmount()
    {
        bool result = new Size(1f, 1f) == new Size(1f + 1e-6f, 1f);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task IsApproximatelyShouldAcceptASizeWithinTheTolerance()
    {
        bool result = new Size(1f, 1f).IsApproximately(new Size(1f + 1e-6f, 1f), 1e-5f);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task IsApproximatelyShouldRejectASizeOutsideTheTolerance()
    {
        bool result = new Size(1f, 1f).IsApproximately(new Size(1.1f, 1f), 1e-5f);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task ShouldConvertExplicitlyToVector2()
    {
        Vector2 extent = (Vector2)new Size(640f, 360f);

        await extent.Should().BeEqualTo(new Vector2(640f, 360f));
    }

    [Test]
    public async Task DeconstructShouldGiveBothDimensions()
    {
        (float width, float height) = new Size(640f, 360f);

        await (width, height).Should().BeEqualTo((640f, 360f));
    }

    [Test]
    public async Task ToStringShouldWriteBothDimensions()
    {
        string result = new Size(1280.5f, 720f).ToString(null, CultureInfo.InvariantCulture);

        await result.Should().BeEqualTo("(1280.5, 720)");
    }
}
