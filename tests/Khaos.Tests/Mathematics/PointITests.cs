// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;
using System.Globalization;
using System.Numerics;

namespace KappaDuck.Khaos.Tests.Mathematics;

public sealed class PointITests
{
    [Test]
    public async Task SubtractingTwoPointsShouldGiveAVector2()
    {
        PointI cursor = new(640, 360);
        PointI anchor = new(600, 400);

        Vector2 drag = cursor - anchor;

        await drag.Should().BeEqualTo(new Vector2(40f, -40f));
    }

    [Test]
    public async Task SubtractingFarApartPointsShouldNotOverflow()
    {
        Vector2 span = new PointI(int.MaxValue, 0) - new PointI(int.MinValue, 0);

        await span.X.Should().BeEqualTo(4_294_967_295f);
    }

    [Test]
    public async Task DistanceShouldNotOverflowForLargeCoordinates()
    {
        float distance = PointI.Distance(new PointI(0, 0), new PointI(50_000, 0));

        await distance.Should().BeEqualTo(50_000f);
    }

    [Test]
    [Arguments(1, 0, 5, 7)]
    [Arguments(0, -1, 4, 6)]
    public async Task OffsetShouldStepToANeighboringCell(int dx, int dy, int expectedX, int expectedY)
    {
        PointI neighbor = new PointI(4, 7).Offset(dx, dy);

        await neighbor.Should().BeEqualTo(new PointI(expectedX, expectedY));
    }

    [Test]
    public async Task AddingADisplacementShouldGiveAFloatingPointLocation()
    {
        Point result = new PointI(10, 10) + new Vector2(0.5f, -0.25f);

        await result.Should().BeEqualTo(new Point(10.5f, 9.75f));
    }

    [Test]
    public async Task SubtractingADisplacementShouldGiveAFloatingPointLocation()
    {
        Point result = new PointI(10, 10) - new Vector2(0.5f, -0.25f);

        await result.Should().BeEqualTo(new Point(9.5f, 10.25f));
    }

    [Test]
    public async Task ShouldConvertImplicitlyToPoint()
    {
        Point point = new PointI(3, -4);

        await point.Should().BeEqualTo(new Point(3f, -4f));
    }

    [Test]
    public async Task ShouldConvertExplicitlyToVector2()
    {
        Vector2 vector = (Vector2)new PointI(3, -4);

        await vector.Should().BeEqualTo(new Vector2(3f, -4f));
    }

    [Test]
    public async Task SubtractingAPointIFromAPointShouldGiveADisplacement()
    {
        Vector2 result = new Point(10.5f, 3f) - new PointI(10, 1);

        await result.Should().BeEqualTo(new Vector2(0.5f, 2f));
    }

    [Test]
    public async Task EqualityShouldRejectSwappedCoordinates()
    {
        bool result = new PointI(2, 3) == new PointI(3, 2);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task EqualPointsShouldBeFoundInAHashSet()
    {
        HashSet<PointI> walls = [new(2, 3)];

        await walls.Contains(new PointI(2, 3)).Should().BeTrue();
    }

    [Test]
    public async Task DeconstructShouldGiveBothCoordinates()
    {
        (int x, int y) = new PointI(3, 4);

        await (x, y).Should().BeEqualTo((3, 4));
    }

    [Test]
    public async Task ToStringShouldWriteBothCoordinates()
    {
        string result = new PointI(3, -4).ToString(null, CultureInfo.InvariantCulture);

        await result.Should().BeEqualTo("(3, -4)");
    }
}
