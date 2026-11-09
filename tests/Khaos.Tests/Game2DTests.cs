// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Tests;

internal sealed class Game2DTests
{
    private readonly Game2D _game = Game2D.Mock(1280, 720);

    [Test]
    public async Task ExitShouldAskTheGameToStop()
    {
        _game.Exit();

        await _game.ExitRequested.Should().BeTrue();
    }

    [Test]
    public async Task ResetShouldClearTheExit()
    {
        _game.Exit();

        _game.Reset();

        await _game.ExitRequested.Should().BeFalse();
    }

    [Test]
    public async Task ConstructorShouldBuildTheGame()
    {
        Game2D game = Game2D.Mock("Title", 1280, 720);

        await game.WindowOptions.Title.Should().BeEqualTo("Title");
        await game.WindowOptions.Width.Should().BeEqualTo(1280);
        await game.WindowOptions.Height.Should().BeEqualTo(720);
    }

    [Test]
    public async Task ConstructorShouldLeaveTheTitleEmptyWhenTheGameGivesNone()
    {
        Game2D game = Game2D.Mock(1280, 720);

        await game.WindowOptions.Title.Should().BeEmpty();
        await game.WindowOptions.Width.Should().BeEqualTo(1280);
        await game.WindowOptions.Height.Should().BeEqualTo(720);
    }
}
