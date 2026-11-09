// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting;
using KappaDuck.Khaos.Interop.SDL;
using KappaDuck.Khaos.Interop.SDL.Primitives;
using KappaDuck.Khaos.Tests.Helpers;

namespace KappaDuck.Khaos.Tests.Hosting;

[Category(Categories.Integration)]
[NotInParallel(Sequential.Engine)]
internal sealed class KhaosEngineTests
{
    private readonly Mock<Game2D> _game = Game2D.Mock("Test", 1280, 720);

    [After(Test)]
    public void ClearAppContext() => GameMetadata.Clear();

    [Test]
    public void StartShouldCallInitializeOnce()
    {
        using KhaosEngine _ = KhaosEngine.Start(_game);

        _game.Initialize().WasCalled(Times.Once);
    }

    [Test]
    public void StartShouldNotDrawAFrame()
    {
        using KhaosEngine _ = KhaosEngine.Start(_game);

        _game.Draw(Any()).WasNeverCalled();
    }

    [Test]
    public async Task StartShouldThrowWhenAGameIsAlreadyRunning()
    {
        using KhaosEngine _ = KhaosEngine.Start(_game);

        await Assert.That(static () => KhaosEngine.Start(Game2D.Mock(30, 30))).ThrowsExactly<InvalidOperationException>();
    }

    [Test]
    public async Task StartShouldRethrowTheExceptionThrownInInitialize()
    {
        InvalidOperationException thrown = new("The level file is missing.");
        _game.Initialize().Throws(thrown);

        InvalidOperationException rethrown = Assert.Throws<InvalidOperationException>(() => KhaosEngine.Start(_game));

        await rethrown.Should().BeSameReferenceAs(thrown);
    }

    [Test]
    public async Task StartShouldStopTheNativeSessionWhenInitializeThrows()
    {
        _game.Initialize().Throws(new InvalidOperationException("The level file is missing."));

        _ = Assert.Throws<InvalidOperationException>(() => KhaosEngine.Start(_game));

        await SDL3.WasInit(SDL_Subsystems.None).Should().BeEqualTo(SDL_Subsystems.None);
    }

    [Test]
    public void StartShouldLetAnotherGameStartWhenInitializeThrows()
    {
        _game.Initialize().Throws(new InvalidOperationException("The level file is missing."));

        _ = Assert.Throws<InvalidOperationException>(() => KhaosEngine.Start(_game));

        Mock<Game2D> next = Game2D.Mock("next", 1280, 720);

        using KhaosEngine engine = KhaosEngine.Start(next.Object);

        next.Initialize().WasCalled(Times.Once);
    }

    [Test]
    public async Task StartShouldClearTheExitOfThePreviousRun()
    {
        _game.Object.Exit();

        using KhaosEngine engine = KhaosEngine.Start(_game);

        SDL_AppResult result = engine.Iterate();

        await result.Should().BeEqualTo(SDL_AppResult.Continue);
    }

    [Test]
    public async Task StartShouldKeepTheExitAskedInInitialize()
    {
        _game.Initialize().Callback(_game.Object.Exit);

        using KhaosEngine engine = KhaosEngine.Start(_game);

        SDL_AppResult result = engine.Iterate();

        await result.Should().BeEqualTo(SDL_AppResult.Success);
    }

    [Test]
    public void IterateShouldDrawOneFrame()
    {
        using KhaosEngine engine = KhaosEngine.Start(_game);

        _ = engine.Iterate();

        _game.Draw(Any()).WasCalled(Times.Once);
    }

    [Test]
    public async Task IterateShouldContinueWhileTheGameRuns()
    {
        using KhaosEngine engine = KhaosEngine.Start(_game);

        SDL_AppResult result = engine.Iterate();

        await result.Should().BeEqualTo(SDL_AppResult.Continue);
    }

    [Test]
    public async Task IterateShouldSucceedOnceTheGameExits()
    {
        using KhaosEngine engine = KhaosEngine.Start(_game);

        _game.Object.Exit();
        SDL_AppResult result = engine.Iterate();

        await result.Should().BeEqualTo(SDL_AppResult.Success);
    }

    [Test]
    public void IterateShouldNotDrawOnceTheGameExits()
    {
        using KhaosEngine engine = KhaosEngine.Start(_game);
        _game.Object.Exit();

        _ = engine.Iterate();

        _game.Draw(Any()).WasNeverCalled();
    }

    [Test]
    public async Task DisposeShouldStopTheNativeSession()
    {
        KhaosEngine engine = KhaosEngine.Start(_game);

        engine.Dispose();

        await SDL3.WasInit(SDL_Subsystems.None).Should().BeEqualTo(SDL_Subsystems.None);
    }

    [Test]
    public void DisposeShouldLetAnotherGameStart()
    {
        KhaosEngine engine = KhaosEngine.Start(_game);
        engine.Dispose();

        Mock<Game2D> next = Game2D.Mock("next", 1280, 720);

        using KhaosEngine _ = KhaosEngine.Start(next);

        next.Initialize().WasCalled(Times.Once);
    }

    [Test]
    public async Task DisposeShouldNotStopTheEngineStartedAfterIt()
    {
        KhaosEngine first = KhaosEngine.Start(Game2D.Mock("first", 10, 10));
        first.Dispose();

        using KhaosEngine _ = KhaosEngine.Start(_game);

        first.Dispose();

        await NativeSession.Started.Should().BeTrue();
    }

    [Test]
    public async Task StartShouldGiveTheNameOfTheGameToTheSystem()
    {
        AppContext.SetData(GameMetadata.Name, "Duck Quest Deluxe");

        using KhaosEngine _ = KhaosEngine.Start(_game);

        string? name = SDLAppMetadata.Get(SDLAppMetadata.Name);

        await name.Should().BeEqualTo("Duck Quest Deluxe");
    }

    [Test]
    public async Task WindowShouldHaveTheTitleOfTheGame()
    {
        using KhaosEngine engine = KhaosEngine.Start(_game);

        await engine.Window.Title.Should().BeEqualTo(_game.Object.WindowOptions.Title);
    }

    [Test]
    public async Task WindowShouldKeepTheTitleOfTheGameOverItsName()
    {
        AppContext.SetData(GameMetadata.Name, "Duck Quest Deluxe");
        Game2D game = Game2D.Mock("Duck Quest Not Deluxe", 1280, 720);

        using KhaosEngine engine = KhaosEngine.Start(game);

        await engine.Window.Title.Should().BeEqualTo(game.WindowOptions.Title);
    }

    [Test]
    public async Task WindowShouldHaveTheNameOfTheGameWhenTheGameHasNoTitle()
    {
        AppContext.SetData(GameMetadata.Name, "Duck Quest Deluxe");

        using KhaosEngine engine = KhaosEngine.Start(Game2D.Mock(1280, 720));

        await engine.Window.Title.Should().BeEqualTo("Duck Quest Deluxe");
    }

    [Test]
    public async Task WindowShouldHaveNoTitleWhenTheGameHasNeitherATitleNorAName()
    {
        using KhaosEngine engine = KhaosEngine.Start(Game2D.Mock(1280, 720));

        await engine.Window.Title.Should().BeEmpty();
    }
}
