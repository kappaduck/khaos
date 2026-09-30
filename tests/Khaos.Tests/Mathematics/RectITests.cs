// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;
using System.Globalization;

namespace KappaDuck.Khaos.Tests.Mathematics;

public sealed class RectITests
{
    [Test]
    public async Task ConstructorShouldTakeThePositionAndTheSize()
    {
        RectI rect = new(new PointI(10, 20), new SizeI(30, 40));

        await rect.X.Should().BeEqualTo(10);
        await rect.Y.Should().BeEqualTo(20);
        await rect.Width.Should().BeEqualTo(30);
        await rect.Height.Should().BeEqualTo(40);
    }

    [Test]
    public async Task RightShouldBeTheLeftEdgePlusTheWidth()
    {
        RectI rect = new(10, 20, 30, 40);

        await rect.Right.Should().BeEqualTo(40);
    }

    [Test]
    public async Task BottomShouldBeTheTopEdgePlusTheHeight()
    {
        RectI rect = new(10, 20, 30, 40);

        await rect.Bottom.Should().BeEqualTo(60);
    }

    [Test]
    public async Task CenterShouldBeHalfwayBetweenTheEdges()
    {
        RectI rect = new(10, 20, 30, 40);

        await rect.Center.Should().BeEqualTo(new Point(25f, 40f));
    }

    [Test]
    public async Task CenterShouldFallBetweenCellsForAnOddSize()
    {
        RectI rect = new(0, 0, 3, 5);

        await rect.Center.Should().BeEqualTo(new Point(1.5f, 2.5f));
    }

    [Test]
    public async Task AreaShouldMultiplyTheDimensions()
    {
        RectI rect = new(10, 20, 30, 40);

        await rect.Area.Should().BeEqualTo(1200L);
    }

    [Test]
    public async Task AreaShouldNotOverflowForLargeRects()
    {
        RectI rect = new(0, 0, 50_000, 50_000);

        await rect.Area.Should().BeEqualTo(2_500_000_000L);
    }

    [Test]
    [Arguments(10, 5, false)]
    [Arguments(0, 5, true)]
    [Arguments(5, 0, true)]
    [Arguments(-1, 5, true)]
    public async Task IsEmptyShouldBeTrueForAZeroOrNegativeDimension(int width, int height, bool expected)
    {
        RectI rect = new(3, 3, width, height);

        await rect.IsEmpty.Should().BeEqualTo(expected);
    }

    [Test]
    [Arguments(10, 5)]
    [Arguments(0, 5)]
    [Arguments(5, 0)]
    [Arguments(-1, 5)]
    public async Task SizeIsEmptyShouldAgreeWithRectIsEmpty(int width, int height)
    {
        RectI rect = new(3, 3, width, height);

        await rect.Size.IsEmpty.Should().BeEqualTo(rect.IsEmpty);
    }

    [Test]
    [Arguments(0, 0)]
    [Arguments(9, 0)]
    [Arguments(0, 9)]
    [Arguments(9, 9)]
    [Arguments(5, 5)]
    public async Task ContainsShouldIncludeEveryCellInside(int x, int y)
    {
        RectI rect = new(0, 0, 10, 10);

        await rect.Contains(new PointI(x, y)).Should().BeTrue();
    }

    [Test]
    [Arguments(10, 0)]
    [Arguments(0, 10)]
    [Arguments(-1, 5)]
    [Arguments(5, -1)]
    public async Task ContainsShouldExcludeTheRightAndBottomEdges(int x, int y)
    {
        RectI rect = new(0, 0, 10, 10);

        await rect.Contains(new PointI(x, y)).Should().BeFalse();
    }

    [Test]
    public async Task ZeroWidthRectShouldContainNoCell()
    {
        RectI line = new(5, 0, 0, 10);

        await line.Contains(new PointI(5, 5)).Should().BeFalse();
    }

    [Test]
    public async Task EmptyRectShouldContainNoCell()
    {
        RectI empty = new(0, 0, -1, 10);

        await empty.Contains(new PointI(0, 0)).Should().BeFalse();
    }

    [Test]
    public async Task RectShouldContainItself()
    {
        RectI rect = new(10, 20, 30, 40);

        await rect.Contains(rect).Should().BeTrue();
    }

    [Test]
    public async Task RectShouldContainARectTouchingItsCorner()
    {
        RectI screen = new(0, 0, 1280, 720);
        RectI rect = new(1200, 700, 80, 20);

        await screen.Contains(rect).Should().BeTrue();
    }

    [Test]
    public async Task RectShouldNotContainARectCrossingAnEdge()
    {
        RectI screen = new(0, 0, 1280, 720);
        RectI rect = new(1200, 700, 81, 20);

        await screen.Contains(rect).Should().BeFalse();
    }

    [Test]
    public async Task RectShouldNotContainAnEmptyRect()
    {
        RectI screen = new(0, 0, 1280, 720);
        RectI rect = new(10, 10, 0, 10);

        await screen.Contains(rect).Should().BeFalse();
    }

    [Test]
    public async Task ContainsAllShouldBeTrueForNoCell()
    {
        RectI rect = new(10, 20, 30, 40);

        await rect.ContainsAll([]).Should().BeTrue();
    }

    [Test]
    public async Task ContainsAllShouldBeTrueWhenEveryCellIsInside()
    {
        RectI rect = new(0, 0, 10, 10);

        await rect.ContainsAll([new PointI(1, 1), new PointI(9, 9)]).Should().BeTrue();
    }

    [Test]
    public async Task ContainsAllShouldBeFalseWhenOneCellIsOutside()
    {
        RectI rect = new(0, 0, 10, 10);

        await rect.ContainsAll([new PointI(1, 1), new PointI(10, 10)]).Should().BeFalse();
    }

    [Test]
    public async Task ContainsAnyShouldBeFalseForNoCell()
    {
        RectI rect = new(10, 20, 30, 40);

        await rect.ContainsAny([]).Should().BeFalse();
    }

    [Test]
    public async Task ContainsAnyShouldBeTrueWhenOneCellIsInside()
    {
        RectI rect = new(0, 0, 10, 10);

        await rect.ContainsAny([new PointI(20, 20), new PointI(1, 1)]).Should().BeTrue();
    }

    [Test]
    public async Task ContainsAnyShouldBeFalseWhenEveryCellIsOutside()
    {
        RectI rect = new(0, 0, 10, 10);

        await rect.ContainsAny([new PointI(20, 20), new PointI(10, 5)]).Should().BeFalse();
    }

    [Test]
    public async Task RectsSharingAnEdgeShouldNotIntersect()
    {
        RectI left = new(0, 0, 10, 10);
        RectI right = new(10, 0, 10, 10);

        await left.Intersects(right).Should().BeFalse();
    }

    [Test]
    public async Task TryIntersectShouldFailForRectsSharingAnEdge()
    {
        bool found = RectI.TryIntersect(new RectI(0, 0, 10, 10), new RectI(10, 0, 10, 10), out _);

        await found.Should().BeFalse();
    }

    [Test]
    public async Task TryIntersectShouldGiveZeroForRectsSharingAnEdge()
    {
        RectI.TryIntersect(new RectI(0, 0, 10, 10), new RectI(10, 0, 10, 10), out RectI intersection);

        await intersection.Should().BeEqualTo(RectI.Zero);
    }

    [Test]
    public async Task OverlappingRectsShouldIntersect()
    {
        RectI left = new(0, 0, 10, 10);
        RectI right = new(5, 5, 10, 10);

        await left.Intersects(right).Should().BeTrue();
    }

    [Test]
    public async Task TryIntersectShouldGiveTheOverlap()
    {
        RectI.TryIntersect(new RectI(0, 0, 10, 10), new RectI(5, 5, 10, 10), out RectI intersection);

        await intersection.Should().BeEqualTo(new RectI(5, 5, 5, 5));
    }

    [Test]
    public async Task SeparateRectsShouldNotIntersect()
    {
        RectI left = new(0, 0, 10, 10);
        RectI right = new(11, 0, 10, 10);

        await left.Intersects(right).Should().BeFalse();
    }

    [Test]
    public async Task TryIntersectShouldFailForSeparateRects()
    {
        bool found = RectI.TryIntersect(new RectI(0, 0, 10, 10), new RectI(11, 0, 10, 10), out _);

        await found.Should().BeFalse();
    }

    [Test]
    public async Task TryIntersectShouldGiveZeroForSeparateRects()
    {
        RectI.TryIntersect(new RectI(0, 0, 10, 10), new RectI(11, 0, 10, 10), out RectI intersection);

        await intersection.Should().BeEqualTo(RectI.Zero);
    }

    [Test]
    public async Task UnionShouldCoverBothRects()
    {
        RectI union = RectI.Union(new RectI(0, 0, 10, 10), new RectI(20, 5, 10, 10));

        await union.Should().BeEqualTo(new RectI(0, 0, 30, 15));
    }

    [Test]
    public async Task UnionShouldIgnoreAnEmptyFirstRect()
    {
        RectI rect = new(10, 20, 30, 40);
        RectI union = RectI.Union(new RectI(100, 100, 0, 0), rect);

        await union.Should().BeEqualTo(rect);
    }

    [Test]
    public async Task UnionShouldIgnoreAnEmptySecondRect()
    {
        RectI rect = new(10, 20, 30, 40);
        RectI union = RectI.Union(rect, new RectI(100, 100, 0, 0));

        await union.Should().BeEqualTo(rect);
    }

    [Test]
    public async Task UnionOfTwoEmptyRectsShouldBeZero()
    {
        RectI empty = new(100, 100, 0, 0);

        await RectI.Union(empty, empty).Should().BeEqualTo(RectI.Zero);
    }

    [Test]
    public async Task UnionWithCellsShouldGrowABoundingBoxFromAnEmptyRect()
    {
        RectI bounds = RectI.Zero;

        bounds = RectI.Union(bounds, new PointI(1, 1));
        bounds = RectI.Union(bounds, new PointI(5, 5));
        bounds = RectI.Union(bounds, new PointI(9, 3));

        await bounds.Should().BeEqualTo(RectI.FromEdges(1, 1, 10, 6));
    }

    [Test]
    public async Task UnionWithACellShouldContainThatCell()
    {
        RectI room = new(4, 4, 8, 6);
        PointI cell = new(9, 3);

        RectI bounds = RectI.Union(room, cell);

        await bounds.Contains(cell).Should().BeTrue();
    }

    [Test]
    public async Task TryEncloseShouldCoverEveryCell()
    {
        RectI.TryEnclose([new PointI(1, 1), new PointI(5, 5), new PointI(9, 3)], out RectI bounds);

        await bounds.Should().BeEqualTo(RectI.FromEdges(1, 1, 10, 6));
    }

    [Test]
    public async Task TryEncloseShouldGiveOneCellForASingleCell()
    {
        RectI.TryEnclose([new PointI(4, 2)], out RectI bounds);

        await bounds.Should().BeEqualTo(new RectI(4, 2, 1, 1));
    }

    [Test]
    public async Task TryEncloseShouldSucceedWithCells()
    {
        bool found = RectI.TryEnclose([new PointI(4, 2)], out _);

        await found.Should().BeTrue();
    }

    [Test]
    public async Task TryEncloseShouldFailWithoutCells()
    {
        bool found = RectI.TryEnclose([], out _);

        await found.Should().BeFalse();
    }

    [Test]
    public async Task TryEncloseShouldGiveZeroWithoutCells()
    {
        RectI.TryEnclose([], out RectI bounds);

        await bounds.Should().BeEqualTo(RectI.Zero);
    }

    [Test]
    public async Task OffsetShouldMoveTheRect()
    {
        RectI moved = new RectI(10, 10, 5, 5).Offset(-10, 3);

        await moved.Should().BeEqualTo(new RectI(0, 13, 5, 5));
    }

    [Test]
    public async Task ShouldConvertImplicitlyToRectWithTheSameEdges()
    {
        Rect rect = new RectI(1, 2, 3, 4);

        await rect.Should().BeEqualTo(new Rect(1f, 2f, 3f, 4f));
    }

    [Test]
    public async Task EqualityShouldAcceptIdenticalRects()
    {
        RectI empty = new(5, 5, 0, 10);

        bool result = empty == new RectI(5, 5, 0, 10);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task EqualityShouldTellEmptyRectsApart()
    {
        bool result = new RectI(5, 5, 0, 10) == new RectI(0, 0, 0, 10);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task EqualRectsShouldBeFoundInAHashSet()
    {
        HashSet<RectI> dirty = [new(0, 0, 32, 32)];

        await dirty.Contains(new RectI(0, 0, 32, 32)).Should().BeTrue();
    }

    [Test]
    public async Task DeconstructShouldGiveEveryComponent()
    {
        RectI rect = new(10, 20, 30, 40);
        (int x, int y, int width, int height) = rect;

        await (x, y, width, height).Should().BeEqualTo((10, 20, 30, 40));
    }

    [Test]
    public async Task ToStringShouldSeparateWithCommasForTheInvariantCulture()
    {
        string result = new RectI(-1, 2, 30, 40).ToString(null, CultureInfo.InvariantCulture);

        await result.Should().BeEqualTo("(-1, 2, 30, 40)");
    }

    [Test]
    public async Task ToStringShouldSeparateWithSemicolonsWhenTheDecimalSeparatorIsAComma()
    {
        string result = new RectI(-1, 2, 30, 40).ToString(null, CultureInfo.GetCultureInfo("fr-CA"));

        await result.Should().BeEqualTo("(-1; 2; 30; 40)");
    }

    [Test]
    public async Task ContainsAllShouldBeTrueForAListInside()
    {
        RectI area = new(0, 0, 10, 10);
        List<PointI> points = [new PointI(1, 1), new PointI(9, 9)];

        await area.ContainsAll(points).Should().BeTrue();
    }

    [Test]
    public async Task ContainsAllShouldBeTrueForAnArrayPassedAsASequence()
    {
        RectI area = new(0, 0, 10, 10);

        PointI[] array = [new PointI(1, 1), new PointI(9, 9)];
        IEnumerable<PointI> points = array;

        await area.ContainsAll(points).Should().BeTrue();
    }

    [Test]
    public async Task ContainsAllShouldBeFalseForASequenceWithOneCellOutside()
    {
        RectI area = new(0, 0, 10, 10);

        await area.ContainsAll(Lazily(new PointI(1, 1), new PointI(10, 10))).Should().BeFalse();
    }

    [Test]
    public async Task ContainsAllShouldBeTrueForAnEmptySequence()
    {
        RectI area = new(0, 0, 10, 10);

        await area.ContainsAll(Lazily()).Should().BeTrue();
    }

    [Test]
    public async Task ContainsAnyShouldBeTrueForASequenceWithOneCellInside()
    {
        RectI area = new(0, 0, 10, 10);

        await area.ContainsAny(Lazily(new PointI(20, 20), new PointI(9, 9))).Should().BeTrue();
    }

    [Test]
    public async Task ContainsAnyShouldBeFalseForAListOutside()
    {
        RectI area = new(0, 0, 10, 10);
        List<PointI> points = [new PointI(20, 20), new PointI(10, 5)];

        await area.ContainsAny(points).Should().BeFalse();
    }

    [Test]
    public async Task TryEncloseShouldGiveTheBoundsOfASequence()
    {
        RectI.TryEnclose(Lazily(new PointI(1, 1), new PointI(5, 5), new PointI(9, 3)), out RectI bounds);

        await bounds.Should().BeEqualTo(RectI.FromEdges(1, 1, 10, 6));
    }

    [Test]
    public async Task TryEncloseShouldGiveTheBoundsOfAList()
    {
        List<PointI> points = [new PointI(1, 1), new PointI(5, 5), new PointI(9, 3)];

        RectI.TryEnclose(points, out RectI bounds);

        await bounds.Should().BeEqualTo(RectI.FromEdges(1, 1, 10, 6));
    }

    [Test]
    public async Task TryEncloseShouldFailForAnEmptySequence()
    {
        bool found = RectI.TryEnclose(Lazily(), out _);

        await found.Should().BeFalse();
    }

    private static IEnumerable<PointI> Lazily(params PointI[] points)
    {
        foreach (PointI point in points)
            yield return point;
    }
}
