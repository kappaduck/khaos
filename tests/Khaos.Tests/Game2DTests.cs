// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Windowing;

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
    public async Task ApplyToShouldGiveTheTitleAndTheSizeOfTheGame()
    {
        Game2D game = Game2D.Mock("Title", 1280, 720);
        WindowOptions options = new();

        game.ApplyTo(options);

        await options.Title.Should().BeEqualTo("Title");
        await options.Width.Should().BeEqualTo(1280);
        await options.Height.Should().BeEqualTo(720);
    }

    [Test]
    public async Task ApplyToShouldKeepTheTitleWhenTheGameGivesNone()
    {
        WindowOptions options = new() { Title = "Duck Quest" };

        _game.ApplyTo(options);

        await options.Title.Should().BeEqualTo("Duck Quest");
    }

    [Test]
    public async Task WindowShouldThrowBeforeTheWindowOpens()
    {
        await Assert.That(() => _game.Window).Throws<InvalidOperationException>().WithMessage(Window.NotOpenYet);
    }
}
