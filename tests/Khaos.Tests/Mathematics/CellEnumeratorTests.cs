// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;

namespace KappaDuck.Khaos.Tests.Mathematics;

public sealed class CellEnumeratorTests
{
    [Test]
    public async Task ShouldGoRowByRowFromTheTopLeft()
    {
        RectI rect = new(10, 10, 2, 2);
        List<PointI> cells = [];

        foreach (PointI cell in rect.Cells)
            cells.Add(cell);

        await cells.SequenceEqual([new(10, 10), new(11, 10), new(10, 11), new(11, 11)]).Should().BeTrue();
    }

    [Test]
    public async Task ShouldEnumerateAsManyCellsAsTheArea()
    {
        RectI rect = new(-2, 3, 4, 3);
        int count = 0;

        foreach (PointI _ in rect.Cells)
            count++;

        await count.Should().BeEqualTo((int)rect.Area);
    }

    [Test]
    public async Task EveryCellShouldBeInsideTheRect()
    {
        RectI rect = new(-2, 3, 4, 3);
        bool allInside = true;

        foreach (PointI cell in rect.Cells)
            allInside &= rect.Contains(cell);

        await allInside.Should().BeTrue();
    }

    [Test]
    [Arguments(0, 5)]
    [Arguments(5, 0)]
    [Arguments(-3, 5)]
    public async Task EmptyRectShouldHaveNoCell(int width, int height)
    {
        RectI rect = new(1, 1, width, height);
        int count = 0;

        foreach (PointI _ in rect.Cells)
            count++;

        await count.Should().BeZero();
    }

    [Test]
    public async Task MoveNextShouldBeFalseAfterTheLastCell()
    {
        CellEnumerator cells = new RectI(0, 0, 1, 1).Cells;

        cells.MoveNext();

        await cells.MoveNext().Should().BeFalse();
    }

    [Test]
    public async Task ResetShouldStartTheCellsOver()
    {
        CellEnumerator cells = new RectI(10, 10, 2, 2).Cells;

        cells.MoveNext();
        cells.MoveNext();
        cells.MoveNext();

        cells.Reset();
        cells.MoveNext();

        await cells.Current.Should().BeEqualTo(new PointI(10, 10));
    }
}
