// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Windowing;

namespace KappaDuck.Khaos.Tests.Windowing;

internal sealed class DisplayKeyTests
{
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
}
