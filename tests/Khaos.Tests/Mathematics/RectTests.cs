// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;
using System.Globalization;
using System.Numerics;

namespace KappaDuck.Khaos.Tests.Mathematics;

public sealed class RectTests
{
    [Test]
    public async Task ConstructorShouldTakeThePositionAndTheSize()
    {
        Rect rect = new(new Point(10f, 20f), new Size(30f, 40f));

        await rect.X.Should().BeEqualTo(10f);
        await rect.Y.Should().BeEqualTo(20f);
        await rect.Width.Should().BeEqualTo(30f);
        await rect.Height.Should().BeEqualTo(40f);
    }

    [Test]
    public async Task RightShouldBeTheLeftEdgePlusTheWidth()
    {
        Rect rect = new(10f, 20f, 30f, 40f);

        await rect.Right.Should().BeEqualTo(40f);
    }

    [Test]
    public async Task BottomShouldBeTheTopEdgePlusTheHeight()
    {
        Rect rect = new(10f, 20f, 30f, 40f);

        await rect.Bottom.Should().BeEqualTo(60f);
    }

    [Test]
    public async Task CenterShouldBeHalfwayBetweenTheEdges()
    {
        Rect rect = new(10f, 20f, 30f, 40f);

        await rect.Center.Should().BeEqualTo(new Point(25f, 40f));
    }

    [Test]
    public async Task TopLeftShouldBeThePosition()
    {
        Rect rect = new(10f, 20f, 30f, 40f);

        await rect.TopLeft.Should().BeEqualTo(new Point(10f, 20f));
    }

    [Test]
    public async Task TopRightShouldBeOnTheRightEdge()
    {
        Rect rect = new(10f, 20f, 30f, 40f);

        await rect.TopRight.Should().BeEqualTo(new Point(40f, 20f));
    }

    [Test]
    public async Task BottomLeftShouldBeOnTheBottomEdge()
    {
        Rect rect = new(10f, 20f, 30f, 40f);

        await rect.BottomLeft.Should().BeEqualTo(new Point(10f, 60f));
    }

    [Test]
    public async Task BottomRightShouldBeOnBothFarEdges()
    {
        Rect rect = new(10f, 20f, 30f, 40f);

        await rect.BottomRight.Should().BeEqualTo(new Point(40f, 60f));
    }

    [Test]
    public async Task AreaShouldMultiplyTheDimensions()
    {
        Rect rect = new(10f, 20f, 30f, 40f);

        await rect.Area.Should().BeEqualTo(1200f);
    }

    [Test]
    [Arguments(0f, 0f, false)]
    [Arguments(0f, 5f, false)]
    [Arguments(-1f, 5f, true)]
    [Arguments(5f, -1f, true)]
    public async Task IsEmptyShouldBeTrueOnlyForANegativeDimension(float width, float height, bool expected)
    {
        Rect rect = new(3f, 3f, width, height);

        await rect.IsEmpty.Should().BeEqualTo(expected);
    }

    [Test]
    [Arguments(0f, 0f)]
    [Arguments(0f, 5f)]
    [Arguments(-1f, 5f)]
    [Arguments(5f, -1f)]
    public async Task SizeIsEmptyShouldAgreeWithRectIsEmpty(float width, float height)
    {
        Rect rect = new(3f, 3f, width, height);

        await rect.Size.IsEmpty.Should().BeEqualTo(rect.IsEmpty);
    }

    [Test]
    [Arguments(0f, 0f)]
    [Arguments(10f, 0f)]
    [Arguments(0f, 10f)]
    [Arguments(10f, 10f)]
    [Arguments(5f, 5f)]
    public async Task ContainsShouldIncludeEveryEdge(float x, float y)
    {
        Rect rect = new(0f, 0f, 10f, 10f);

        await rect.Contains(new Point(x, y)).Should().BeTrue();
    }

    [Test]
    [Arguments(10.001f, 5f)]
    [Arguments(5f, -0.001f)]
    public async Task ContainsShouldExcludePointsPastAnEdge(float x, float y)
    {
        Rect rect = new(0f, 0f, 10f, 10f);

        await rect.Contains(new Point(x, y)).Should().BeFalse();
    }

    [Test]
    public async Task ZeroWidthRectShouldContainThePointsOnItsLine()
    {
        Rect line = new(5f, 0f, 0f, 10f);

        await line.Contains(new Point(5f, 5f)).Should().BeTrue();
    }

    [Test]
    public async Task EmptyRectShouldContainNoPoint()
    {
        Rect empty = new(0f, 0f, -1f, 10f);

        await empty.Contains(new Point(0f, 0f)).Should().BeFalse();
    }

    [Test]
    public async Task RectShouldContainItself()
    {
        Rect rect = new(10f, 20f, 30f, 40f);

        await rect.Contains(rect).Should().BeTrue();
    }

    [Test]
    public async Task RectShouldContainARectTouchingItsCorner()
    {
        Rect screen = new(0f, 0f, 1280f, 720f);
        Rect rect = new(1200f, 700f, 80f, 20f);

        await screen.Contains(rect).Should().BeTrue();
    }

    [Test]
    public async Task RectShouldNotContainARectCrossingAnEdge()
    {
        Rect screen = new(0f, 0f, 1280f, 720f);
        Rect rect = new(1200f, 700f, 81f, 20f);

        await screen.Contains(rect).Should().BeFalse();
    }

    [Test]
    public async Task RectShouldNotContainAnEmptyRect()
    {
        Rect screen = new(0f, 0f, 1280f, 720f);
        Rect rect = new(10f, 10f, -1f, 10f);

        await screen.Contains(rect).Should().BeFalse();
    }

    [Test]
    public async Task ContainsAllShouldBeTrueForNoPoint()
    {
        Rect rect = new(10f, 20f, 30f, 40f);

        await rect.ContainsAll([]).Should().BeTrue();
    }

    [Test]
    public async Task ContainsAllShouldBeTrueWhenEveryPointIsInside()
    {
        Rect rect = new(0f, 0f, 10f, 10f);

        await rect.ContainsAll([new Point(1f, 1f), new Point(10f, 10f)]).Should().BeTrue();
    }

    [Test]
    public async Task ContainsAllShouldBeFalseWhenOnePointIsOutside()
    {
        Rect rect = new(0f, 0f, 10f, 10f);

        await rect.ContainsAll([new Point(1f, 1f), new Point(20f, 20f)]).Should().BeFalse();
    }

    [Test]
    public async Task ContainsAnyShouldBeFalseForNoPoint()
    {
        Rect rect = new(10f, 20f, 30f, 40f);

        await rect.ContainsAny([]).Should().BeFalse();
    }

    [Test]
    public async Task ContainsAnyShouldBeTrueWhenOnePointIsInside()
    {
        Rect rect = new(0f, 0f, 10f, 10f);

        await rect.ContainsAny([new Point(20f, 20f), new Point(1f, 1f)]).Should().BeTrue();
    }

    [Test]
    public async Task ContainsAnyShouldBeFalseWhenEveryPointIsOutside()
    {
        Rect rect = new(0f, 0f, 10f, 10f);

        await rect.ContainsAny([new Point(20f, 20f), new Point(-1f, 5f)]).Should().BeFalse();
    }

    [Test]
    public async Task RectsSharingAnEdgeShouldIntersect()
    {
        Rect left = new(0f, 0f, 10f, 10f);

        await left.Intersects(new Rect(10f, 0f, 10f, 10f)).Should().BeTrue();
    }

    [Test]
    public async Task TryIntersectShouldSucceedForRectsSharingAnEdge()
    {
        bool found = Rect.TryIntersect(new Rect(0f, 0f, 10f, 10f), new Rect(10f, 0f, 10f, 10f), out _);

        await found.Should().BeTrue();
    }

    [Test]
    public async Task TryIntersectShouldGiveTheSharedEdgeForRectsSharingAnEdge()
    {
        Rect.TryIntersect(new Rect(0f, 0f, 10f, 10f), new Rect(10f, 0f, 10f, 10f), out Rect intersection);

        await intersection.Should().BeEqualTo(new Rect(10f, 0f, 0f, 10f));
    }

    [Test]
    public async Task OverlappingRectsShouldIntersect()
    {
        Rect left = new(0f, 0f, 10f, 10f);

        await left.Intersects(new Rect(5f, 5f, 10f, 10f)).Should().BeTrue();
    }

    [Test]
    public async Task TryIntersectShouldGiveTheOverlap()
    {
        Rect.TryIntersect(new Rect(0f, 0f, 10f, 10f), new Rect(5f, 5f, 10f, 10f), out Rect intersection);

        await intersection.Should().BeEqualTo(new Rect(5f, 5f, 5f, 5f));
    }

    [Test]
    public async Task SeparateRectsShouldNotIntersect()
    {
        Rect left = new(0f, 0f, 10f, 10f);
        Rect right = new(10.5f, 0f, 10f, 10f);

        await left.Intersects(right).Should().BeFalse();
    }

    [Test]
    public async Task TryIntersectShouldFailForSeparateRects()
    {
        bool found = Rect.TryIntersect(new Rect(0f, 0f, 10f, 10f), new Rect(10.5f, 0f, 10f, 10f), out _);

        await found.Should().BeFalse();
    }

    [Test]
    public async Task TryIntersectShouldGiveZeroForSeparateRects()
    {
        Rect.TryIntersect(new Rect(0f, 0f, 10f, 10f), new Rect(10.5f, 0f, 10f, 10f), out Rect intersection);

        await intersection.Should().BeEqualTo(Rect.Zero);
    }

    [Test]
    public async Task UnionShouldCoverBothRects()
    {
        Rect union = Rect.Union(new Rect(0f, 0f, 10f, 10f), new Rect(20f, 5f, 10f, 10f));

        await union.Should().BeEqualTo(new Rect(0f, 0f, 30f, 15f));
    }

    [Test]
    public async Task UnionShouldIgnoreAnEmptyFirstRect()
    {
        Rect rect = new(10f, 20f, 30f, 40f);
        Rect union = Rect.Union(new Rect(100f, 100f, -1f, -1f), rect);

        await union.Should().BeEqualTo(rect);
    }

    [Test]
    public async Task UnionShouldIgnoreAnEmptySecondRect()
    {
        Rect rect = new(10f, 20f, 30f, 40f);
        Rect union = Rect.Union(rect, new Rect(100f, 100f, -1f, -1f));

        await union.Should().BeEqualTo(rect);
    }

    [Test]
    public async Task UnionOfTwoEmptyRectsShouldBeZero()
    {
        Rect empty = new(100f, 100f, -1f, -1f);

        await Rect.Union(empty, empty).Should().BeEqualTo(Rect.Zero);
    }

    [Test]
    public async Task UnionWithPointsShouldGrowABoundingBoxFromAnEmptyRect()
    {
        Rect bounds = new(0f, 0f, -1f, -1f);

        bounds = Rect.Union(bounds, new Point(1f, 1f));
        bounds = Rect.Union(bounds, new Point(5f, 5f));
        bounds = Rect.Union(bounds, new Point(9f, 3f));

        await bounds.Should().BeEqualTo(Rect.FromEdges(1f, 1f, 9f, 5f));
    }

    [Test]
    public async Task TryEncloseShouldGiveTheBoundsOfEveryPoint()
    {
        Rect.TryEnclose([new Point(1f, 1f), new Point(5f, 5f), new Point(9f, 3f)], out Rect bounds);

        await bounds.Should().BeEqualTo(Rect.FromEdges(1f, 1f, 9f, 5f));
    }

    [Test]
    public async Task TryEncloseShouldGiveAZeroSizeRectForASinglePoint()
    {
        Rect.TryEnclose([new Point(4f, 2f)], out Rect bounds);

        await bounds.Should().BeEqualTo(new Rect(4f, 2f, 0f, 0f));
    }

    [Test]
    public async Task TryEncloseShouldSucceedWithPoints()
    {
        bool found = Rect.TryEnclose([new Point(4f, 2f)], out _);

        await found.Should().BeTrue();
    }

    [Test]
    public async Task TryEncloseShouldFailWithoutPoints()
    {
        bool found = Rect.TryEnclose([], out _);

        await found.Should().BeFalse();
    }

    [Test]
    public async Task TryEncloseShouldGiveZeroWithoutPoints()
    {
        Rect.TryEnclose([], out Rect bounds);

        await bounds.Should().BeEqualTo(Rect.Zero);
    }

    [Test]
    public async Task RoundShouldKeepTwoRectsSharingAnEdgeAdjacent()
    {
        Rect left = new(0.6f, 0f, 0.8f, 1f);
        Rect right = new(1.4f, 0f, 1f, 1f);

        await left.Round().Right.Should().BeEqualTo(right.Round().Left);
    }

    [Test]
    public async Task RoundShouldSendHalvesAwayFromZero()
    {
        RectI rounded = new Rect(0.5f, -0.5f, 1f, 1f).Round();

        await rounded.Should().BeEqualTo(RectI.FromEdges(1, -1, 2, 1));
    }

    [Test]
    public async Task RoundOutShouldCoverEveryTouchedCell()
    {
        RectI cells = new Rect(0.5f, 0.5f, 1f, 1f).RoundOut();

        await cells.Should().BeEqualTo(new RectI(0, 0, 2, 2));
    }

    [Test]
    public async Task OffsetShouldMoveTheRect()
    {
        Rect moved = new Rect(10f, 10f, 5f, 5f).Offset(new Vector2(-10f, 2.5f));

        await moved.Should().BeEqualTo(new Rect(0f, 12.5f, 5f, 5f));
    }

    [Test]
    public async Task EqualityShouldAcceptIdenticalRects()
    {
        Rect empty = new(5f, 5f, -1f, 10f);

        bool result = empty == new Rect(5f, 5f, -1f, 10f);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task EqualityShouldTellEmptyRectsApart()
    {
        bool result = new Rect(5f, 5f, -1f, 10f) == new Rect(0f, 0f, -1f, 10f);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task EqualRectsShouldBeFoundInAHashSet()
    {
        HashSet<Rect> dirty = [new(0f, 0f, 32f, 32f)];

        await dirty.Contains(new Rect(0f, 0f, 32f, 32f)).Should().BeTrue();
    }

    [Test]
    public async Task IsApproximatelyShouldAcceptARectWithinTheTolerance()
    {
        bool result = new Rect(0f, 0f, 10f, 10f).IsApproximately(new Rect(0.01f, 0f, 10f, 9.99f), 0.02f);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task IsApproximatelyShouldRejectARectOutsideTheTolerance()
    {
        bool result = new Rect(0f, 0f, 10f, 10f).IsApproximately(new Rect(0f, 0f, 10.1f, 10f), 0.02f);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task DeconstructShouldGiveEveryComponent()
    {
        Rect rect = new(10f, 20f, 30f, 40f);
        (float x, float y, float width, float height) = rect;

        await (x, y, width, height).Should().BeEqualTo((10f, 20f, 30f, 40f));
    }

    [Test]
    public async Task ToStringShouldSeparateWithCommasForTheInvariantCulture()
    {
        string result = new Rect(0.5f, 1f, 2.25f, 3f).ToString(null, CultureInfo.InvariantCulture);

        await result.Should().BeEqualTo("(0.5, 1, 2.25, 3)");
    }

    [Test]
    public async Task ToStringShouldSeparateWithSemicolonsWhenTheDecimalSeparatorIsAComma()
    {
        string result = new Rect(0.5f, 1f, 2.25f, 3f).ToString(null, CultureInfo.GetCultureInfo("fr-CA"));

        await result.Should().BeEqualTo("(0,5; 1; 2,25; 3)");
    }

    [Test]
    public async Task ContainsAllShouldBeTrueForAListInside()
    {
        Rect area = new(0f, 0f, 10f, 10f);
        List<Point> points = [new Point(1f, 1f), new Point(10f, 10f)];

        await area.ContainsAll(points).Should().BeTrue();
    }

    [Test]
    public async Task ContainsAllShouldBeTrueForAnArrayPassedAsASequence()
    {
        Rect area = new(0f, 0f, 10f, 10f);

        Point[] array = [new Point(1f, 1f), new Point(10f, 10f)];
        IEnumerable<Point> points = array;

        await area.ContainsAll(points).Should().BeTrue();
    }

    [Test]
    public async Task ContainsAllShouldBeFalseForASequenceWithOnePointOutside()
    {
        Rect area = new(0f, 0f, 10f, 10f);

        await area.ContainsAll(Lazily(new Point(1f, 1f), new Point(20f, 20f))).Should().BeFalse();
    }

    [Test]
    public async Task ContainsAllShouldBeTrueForAnEmptySequence()
    {
        Rect area = new(0f, 0f, 10f, 10f);

        await area.ContainsAll(Lazily()).Should().BeTrue();
    }

    [Test]
    public async Task ContainsAnyShouldBeTrueForASequenceWithOnePointInside()
    {
        Rect area = new(0f, 0f, 10f, 10f);

        await area.ContainsAny(Lazily(new Point(20f, 20f), new Point(10f, 10f))).Should().BeTrue();
    }

    [Test]
    public async Task ContainsAnyShouldBeFalseForAListOutside()
    {
        Rect area = new(0f, 0f, 10f, 10f);
        List<Point> points = [new Point(20f, 20f), new Point(-1f, 5f)];

        await area.ContainsAny(points).Should().BeFalse();
    }

    [Test]
    public async Task TryEncloseShouldGiveTheBoundsOfASequence()
    {
        Rect.TryEnclose(Lazily(new Point(1f, 1f), new Point(5f, 5f), new Point(9f, 3f)), out Rect bounds);

        await bounds.Should().BeEqualTo(Rect.FromEdges(1f, 1f, 9f, 5f));
    }

    [Test]
    public async Task TryEncloseShouldGiveTheBoundsOfAList()
    {
        List<Point> points = [new Point(1f, 1f), new Point(5f, 5f), new Point(9f, 3f)];

        Rect.TryEnclose(points, out Rect bounds);

        await bounds.Should().BeEqualTo(Rect.FromEdges(1f, 1f, 9f, 5f));
    }

    [Test]
    public async Task TryEncloseShouldFailForAnEmptySequence()
    {
        bool found = Rect.TryEnclose(Lazily(), out _);

        await found.Should().BeFalse();
    }

    private static IEnumerable<Point> Lazily(params Point[] points)
    {
        foreach (Point point in points)
            yield return point;
    }
}
