// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting;
using KappaDuck.Khaos.Interop.SDL;
using KappaDuck.Khaos.Interop.SDL.Primitives;
using KappaDuck.Khaos.Tests.Helpers;

namespace KappaDuck.Khaos.Tests.Hosting;

[Category(Categories.Integration)]
[NotInParallel(Sequential.Engine)]
internal sealed class GameRunnerTests
{
    private readonly Mock<Game2D> _game = Game2D.Mock("Test", 1280, 720);

    public GameRunnerTests()
    {
        ExitAt(_game, 1);
    }

    [Test]
    public async Task RunShouldReturnZeroWhenTheGameExits()
    {
        int exitCode = GameRunner.Run(_game);

        await exitCode.Should().BeZero();
    }

    [Test]
    public void RunShouldStopDrawingOnceTheGameExits()
    {
        Mock<Game2D> game = Game2D.Mock(1280, 720);
        ExitAt(game, 3);

        GameRunner.Run(game);

        game.Draw(Any()).WasCalled(Times.Exactly(3));
    }

    [Test]
    public async Task RunShouldCallInitializeBeforeTheFirstDraw()
    {
        GameRunner.Run(_game);

        await Assert.That(() => Mock.VerifyInOrder(() =>
        {
            _game.Initialize().WasCalled();
            _game.Draw(Any()).WasCalled();
        })).ThrowsNothing();
    }

    [Test]
    public async Task RunShouldRethrowTheExceptionThrownInDraw()
    {
        InvalidOperationException thrown = new("The duck fell off the map.");

        _game.Draw(Any()).Throws(thrown);

        InvalidOperationException rethrown = Assert.Throws<InvalidOperationException>(() => GameRunner.Run(_game));

        await rethrown.Should().BeSameReferenceAs(thrown);
    }

    [Test]
    public async Task RunShouldKeepTheStackTraceOfTheExceptionThrownInDraw()
    {
        _game.Draw(Any()).Callback(FallOffTheMap);

        InvalidOperationException rethrown = Assert.Throws<InvalidOperationException>(() => GameRunner.Run(_game));

        await rethrown.StackTrace.Should().Contain(nameof(FallOffTheMap));
    }

    [Test]
    public async Task RunShouldRethrowTheExceptionThrownInInitialize()
    {
        InvalidOperationException thrown = new("The level file is missing.");
        _game.Initialize().Throws(thrown);

        InvalidOperationException rethrown = Assert.Throws<InvalidOperationException>(() => GameRunner.Run(_game));

        await rethrown.Should().BeSameReferenceAs(thrown);
    }

    [Test]
    public async Task RunShouldQuitTheEngineWhenTheGameExits()
    {
        GameRunner.Run(_game);

        await SDL3.WasInit(SDL_Subsystems.None).Should().BeEqualTo(SDL_Subsystems.None);
    }

    [Test]
    public async Task RunShouldQuitTheEngineWhenDrawThrows()
    {
        _game.Draw(Any()).Throws(new InvalidOperationException("The duck fell off the map."));

        _ = Assert.Throws<InvalidOperationException>(() => GameRunner.Run(_game));

        await SDL3.WasInit(SDL_Subsystems.None).Should().BeEqualTo(SDL_Subsystems.None);
    }

    [Test]
    public async Task RunShouldQuitTheEngineWhenInitializeThrows()
    {
        _game.Initialize().Throws(new InvalidOperationException("The level file is missing."));

        _ = Assert.Throws<InvalidOperationException>(() => GameRunner.Run(_game));

        await SDL3.WasInit(SDL_Subsystems.None).Should().BeEqualTo(SDL_Subsystems.None);
    }

    [Test]
    public async Task RunShouldThrowWhenAGameIsAlreadyRunning()
    {
        _game.Draw(Any()).Callback(static () => GameRunner.Run(Game2D.Mock(1280, 720)));

        await Assert.That(() => GameRunner.Run(_game)).ThrowsExactly<InvalidOperationException>();
    }

    [Test]
    public async Task RunShouldThrowWhileAnEngineIsStarted()
    {
        using KhaosEngine _ = KhaosEngine.Start(_game);
        Game2D other = Game2D.Mock(1280, 720);

        await Assert.That(() => GameRunner.Run(other)).ThrowsExactly<InvalidOperationException>();
    }

    [Test]
    public async Task RunShouldLeaveTheStartedEngineRunningWhenItThrows()
    {
        using KhaosEngine engine = KhaosEngine.Start(Game2D.Mock(1280, 720));
        Game2D other = Game2D.Mock(1280, 720);

        _ = Assert.Throws<InvalidOperationException>(() => GameRunner.Run(other));

        SDL_Subsystems started = SDL3.WasInit(SDL_Subsystems.Video);

        await started.Should().BeEqualTo(SDL_Subsystems.Video);
    }

    [Test]
    public void RunShouldStopAtTheNextFrameWhenAQuitEventArrives()
    {
        _game.Draw(Any()).Callback(SDLEvents.PushQuit);

        GameRunner.Run(_game);

        _game.Draw(Any()).WasCalled(Times.Once);
    }

    private static void ExitAt(Mock<Game2D> game, int frame)
    {
        int draws = 0;

        game.Draw(Any()).Callback(() =>
        {
            draws++;

            if (draws == frame)
                game.Object.Exit();
        });
    }

    private static void FallOffTheMap() => throw new InvalidOperationException("The duck fell off the map.");
}
