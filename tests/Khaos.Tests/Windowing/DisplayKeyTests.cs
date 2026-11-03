// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;
using KappaDuck.Khaos.Windowing;

namespace KappaDuck.Khaos.Tests.Windowing;

internal sealed class DisplayKeyTests
{
    private static readonly RectI _left = new(0, 0, 1920, 1080);
    private static readonly RectI _middle = new(1920, 0, 1920, 1080);
    private static readonly RectI _right = new(3840, 0, 1920, 1080);

    [Test]
    public async Task OfShouldBeTheNameForTheFirstScreen()
    {
        string key = DisplayKey.Of("U28E590", 1);

        await key.Should().BeEqualTo("U28E590");
    }

    [Test]
    public async Task OfShouldAddTheRankForTheSameName()
    {
        string key = DisplayKey.Of("U28E590", 2);

        await key.Should().BeEqualTo("U28E590 #2");
    }

    [Test]
    public async Task ParseShouldReadAPlainNameAsTheFirstScreen()
    {
        (string name, int rank) = DisplayKey.Parse("U28E590");

        await name.Should().BeEqualTo("U28E590");
        await rank.Should().BeEqualTo(1);
    }

    [Test]
    public async Task ParseShouldReadTheNameAndTheRank()
    {
        (string name, int rank) = DisplayKey.Parse("U28E590 #2");

        await name.Should().BeEqualTo("U28E590");
        await rank.Should().BeEqualTo(2);
    }

    [Test]
    public async Task ParseShouldKeepANameThatEndsWithAHashWithoutANumber()
    {
        (string name, int rank) = DisplayKey.Parse("Studio #A");

        await name.Should().BeEqualTo("Studio #A");
        await rank.Should().BeEqualTo(1);
    }

    [Test]
    public async Task ParseShouldKeepARankOfOneInTheName()
    {
        (string name, int rank) = DisplayKey.Parse("Studio #1");

        await name.Should().BeEqualTo("Studio #1");
        await rank.Should().BeEqualTo(1);
    }

    [Test]
    public async Task PickShouldGiveTheScreenWithThatRank()
    {
        RectI[] screens = [_right, _left, _middle];

        int index = DisplayKey.Pick(2, screens);

        await index.Should().BeEqualTo(2);
    }

    [Test]
    public async Task PickShouldGiveTheRightmostScreenWhenTheRankIsTooHigh()
    {
        RectI[] screens = [_right, _left];

        int index = DisplayKey.Pick(3, screens);

        await index.Should().BeEqualTo(0);
    }

    [Test]
    public async Task PickShouldGiveNoScreenWhenThereIsNone()
    {
        int index = DisplayKey.Pick(1, []);

        await index.Should().BeEqualTo(-1);
    }

    [Test]
    public async Task RankShouldBe1ForTheLeftmostScreen()
    {
        RectI[] screens = [_right, _left];

        int rank = DisplayKey.Rank(_left, screens);

        await rank.Should().BeEqualTo(1);
    }

    [Test]
    public async Task RankShouldCountTheScreensOnTheLeft()
    {
        RectI[] screens = [_right, _left, _middle];

        int rank = DisplayKey.Rank(_middle, screens);

        await rank.Should().BeEqualTo(2);
    }

    [Test]
    public async Task RankShouldPutTheScreenAboveFirstWhenTwoShareTheSameX()
    {
        RectI below = new(0, 1080, 1920, 1080);
        RectI[] screens = [below, _left];

        int rank = DisplayKey.Rank(below, screens);

        await rank.Should().BeEqualTo(2);
    }
}
