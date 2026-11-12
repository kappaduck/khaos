// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;
using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Hosting.Engine;
using KappaDuck.Khaos.Input;
using KappaDuck.Khaos.Interop.SDL;
using KappaDuck.Khaos.Interop.SDL.Primitives;
using KappaDuck.Khaos.Mathematics;
using KappaDuck.Khaos.Windowing;
using Microsoft.Extensions.Logging;

namespace KappaDuck.Khaos.Tests.Hosting.Engine;

[Category(Categories.Integration)]
[NotInParallel(Sequential.Engine)]
internal sealed class KhaosEngineTests
{
    private readonly Mock<Game2D> _game = Game2D.Mock("Test", 1280, 720);

    private const string MainCallbackRate = "SDL_MAIN_CALLBACK_RATE";

    [After(Test)]
    public void ClearAppContext() => GameMetadata.Clear();

    [After(Test)]
    public void ForgetEnvironment() => SDLEnvironment.Unset(MainCallbackRate);

    [Test]
    [OffscreenOnly]
    public async Task StartShouldFallBackWhenTheRenderDriverIsMissing()
    {
        EngineOptions options = new() { RenderDriver = RenderDriver.Direct3D11 };

        using KhaosEngine engine = TestEngine.Start(_game, options);

        await SDLRender.NameOf(engine.Window).Should().NotBeEqualTo("direct3d11");
    }

    [Test]
    public async Task StartShouldUseTheRenderDriver()
    {
        EngineOptions options = new() { RenderDriver = RenderDriver.Software };

        using KhaosEngine engine = TestEngine.Start(_game, options);

        await SDLRender.NameOf(engine.Window).Should().BeEqualTo("software");
    }

    [Test]
    [OffscreenOnly]
    public async Task StartShouldWarnWhenTheRenderDriverIsMissing()
    {
        EngineOptions options = new() { RenderDriver = RenderDriver.Direct3D11 };
        LogRecorder logs = new();
        using ILoggerFactory loggers = logs.Factory(LogLevel.Warning);

        using KhaosEngine _ = TestEngine.Start(_game, options, loggers);

        await logs.Entries.Count(static log => log.Category == KhaosEngine.Category && log.Level == LogLevel.Warning).Should().BeEqualTo(1);
    }

    [Test]
    [OffscreenOnly]
    public async Task StartShouldLogTheDriversByTheirNames()
    {
        EngineOptions options = new() { RenderDriver = RenderDriver.Software };
        LogRecorder logs = new();
        using ILoggerFactory loggers = logs.Factory(LogLevel.Information);

        using KhaosEngine _ = TestEngine.Start(_game, options, loggers);

        RecordedLog started = logs.Entries.Single(static log => log.Category == KhaosEngine.Category && log.Level == LogLevel.Information);
        await started.Message.Should().Contain("started on Offscreen with Software (native ");
    }

    [Test]
    public async Task StartShouldLogTheVersionWithoutItsCommit()
    {
        LogRecorder logs = new();
        using ILoggerFactory loggers = logs.Factory(LogLevel.Information);

        using KhaosEngine _ = TestEngine.Start(_game, loggers);

        RecordedLog started = logs.Entries.Single(static log => log.Category == KhaosEngine.Category && log.Level == LogLevel.Information);
        await started.Message.Should().NotContain("+");
    }

    [Test]
    public async Task StartShouldLetAnEnvironmentVariableWinOverMaxFrameRate()
    {
        SDLEnvironment.Set(MainCallbackRate, "30");
        GraphicsOptions graphics = new() { VSync = VSync.Disabled, MaxFrameRate = 60 };

        using KhaosEngine _ = TestEngine.Start(_game, graphics);

        await SDLHints.Get(MainCallbackRate).Should().BeEqualTo("30");
    }

    [Test]
    public async Task StartShouldLimitTheFrameRate()
    {
        GraphicsOptions graphics = new() { VSync = VSync.Disabled, MaxFrameRate = 30 };

        using KhaosEngine _ = TestEngine.Start(_game, graphics);

        await SDLHints.Get(MainCallbackRate).Should().BeEqualTo("30");
    }

    [Test]
    public async Task StartShouldNotLimitTheFrameRateByDefault()
    {
        using KhaosEngine _ = TestEngine.Start(_game);

        await SDLHints.Get(MainCallbackRate).Should().BeEqualTo("0");
    }

    [Test]
    public void StartShouldCallInitializeOnce()
    {
        using KhaosEngine _ = TestEngine.Start(_game);

        _game.Initialize().WasCalled(Times.Once);
    }

    [Test]
    public void StartShouldNotDrawAFrame()
    {
        using KhaosEngine _ = TestEngine.Start(_game);

        _game.Draw(Any()).WasNeverCalled();
    }

    [Test]
    public async Task StartShouldThrowWhenAGameIsAlreadyRunning()
    {
        using KhaosEngine _ = TestEngine.Start(_game);

        await Assert.That(static () => TestEngine.Start(Game2D.Mock(30, 30))).ThrowsExactly<InvalidOperationException>();
    }

    [Test]
    public async Task StartShouldRethrowTheExceptionThrownInInitialize()
    {
        InvalidOperationException thrown = new("The level file is missing.");
        _game.Initialize().Throws(thrown);

        InvalidOperationException rethrown = Assert.Throws<InvalidOperationException>(() => TestEngine.Start(_game));

        await rethrown.Should().BeSameReferenceAs(thrown);
    }

    [Test]
    public async Task StartShouldStopTheNativeSessionWhenInitializeThrows()
    {
        _game.Initialize().Throws(new InvalidOperationException("The level file is missing."));

        _ = Assert.Throws<InvalidOperationException>(() => TestEngine.Start(_game));

        await SDL3.WasInit(SDL_Subsystems.None).Should().BeEqualTo(SDL_Subsystems.None);
    }

    [Test]
    public void StartShouldLetAnotherGameStartWhenInitializeThrows()
    {
        _game.Initialize().Throws(new InvalidOperationException("The level file is missing."));

        _ = Assert.Throws<InvalidOperationException>(() => TestEngine.Start(_game));

        Mock<Game2D> next = Game2D.Mock("next", 1280, 720);

        using KhaosEngine engine = TestEngine.Start(next.Object);

        next.Initialize().WasCalled(Times.Once);
    }

    [Test]
    public async Task StartShouldClearTheExitOfThePreviousRun()
    {
        _game.Object.Exit();

        using KhaosEngine engine = TestEngine.Start(_game);

        SDL_AppResult result = engine.Iterate();

        await result.Should().BeEqualTo(SDL_AppResult.Continue);
    }

    [Test]
    public async Task StartShouldKeepTheExitAskedInInitialize()
    {
        _game.Initialize().Callback(_game.Object.Exit);

        using KhaosEngine engine = TestEngine.Start(_game);

        SDL_AppResult result = engine.Iterate();

        await result.Should().BeEqualTo(SDL_AppResult.Success);
    }

    [Test]
    public void IterateShouldDrawOneFrame()
    {
        using KhaosEngine engine = TestEngine.Start(_game);

        _ = engine.Iterate();

        _game.Draw(Any()).WasCalled(Times.Once);
    }

    [Test]
    public async Task IterateShouldContinueWhileTheGameRuns()
    {
        using KhaosEngine engine = TestEngine.Start(_game);

        SDL_AppResult result = engine.Iterate();

        await result.Should().BeEqualTo(SDL_AppResult.Continue);
    }

    [Test]
    public async Task IterateShouldSucceedOnceTheGameExits()
    {
        using KhaosEngine engine = TestEngine.Start(_game);

        _game.Object.Exit();
        SDL_AppResult result = engine.Iterate();

        await result.Should().BeEqualTo(SDL_AppResult.Success);
    }

    [Test]
    public void IterateShouldNotDrawOnceTheGameExits()
    {
        using KhaosEngine engine = TestEngine.Start(_game);
        _game.Object.Exit();

        _ = engine.Iterate();

        _game.Draw(Any()).WasNeverCalled();
    }

    [Test]
    public void DispatchShouldPassTheEventToOnEvent()
    {
        using KhaosEngine engine = TestEngine.Start(_game);
        Event e = new(new KeyPressed(Scancode.Space));

        engine.Dispatch(in e);

        _game.OnEvent(received => received is KeyPressed { Code: Scancode.Space }).WasCalled(Times.Once);
    }

    [Test]
    public async Task DispatchShouldExitTheGameOnQuitRequested()
    {
        using KhaosEngine engine = TestEngine.Start(_game);
        Event e = new(new QuitRequested());

        engine.Dispatch(in e);

        await _game.Object.ExitRequested.Should().BeTrue();
    }

    [Test]
    public async Task DispatchShouldCallOnEventBeforeExitingOnQuitRequested()
    {
        bool exitRequestedInOnEvent = true;
        _game.OnEvent(Any()).Callback(() => exitRequestedInOnEvent = _game.Object.ExitRequested);

        using KhaosEngine engine = TestEngine.Start(_game);
        Event e = new(new QuitRequested());

        engine.Dispatch(in e);

        await exitRequestedInOnEvent.Should().BeFalse();
    }

    [Test]
    public async Task DispatchShouldNotExitTheGameOnAnotherEvent()
    {
        using KhaosEngine engine = TestEngine.Start(_game);
        Event e = new(new KeyPressed(Scancode.Escape));

        engine.Dispatch(in e);

        await _game.Object.ExitRequested.Should().BeFalse();
    }

    [Test]
    public void ReceiveShouldDispatchAnEventThatHasACase()
    {
        using KhaosEngine engine = TestEngine.Start(_game);
        SDL_Event native = new(new SDL_KeyboardEvent(SDL_EventType.KeyDown, (int)Scancode.Escape, 0));

        engine.Receive(in native);

        _game.OnEvent(received => received is KeyPressed { Code: Scancode.Escape }).WasCalled(Times.Once);
    }

    [Test]
    public void ReceiveShouldIgnoreAKeyRepeat()
    {
        using KhaosEngine engine = TestEngine.Start(_game);
        SDL_Event native = new(new SDL_KeyboardEvent(SDL_EventType.KeyDown, (int)Scancode.Escape, 1));

        engine.Receive(in native);

        _game.OnEvent(Any()).WasNeverCalled();
    }

    [Test]
    public void ReceiveShouldIgnoreATypeWithoutACase()
    {
        using KhaosEngine engine = TestEngine.Start(_game);
        SDL_Event native = new(SDL_EventType.WindowPixelSizeChanged);

        engine.Receive(in native);

        _game.OnEvent(Any()).WasNeverCalled();
    }

    [Test]
    public void ReceiveShouldGiveTheSizeOfAResizeInLogicalUnits()
    {
        using KhaosEngine engine = TestEngine.Start(_game);
        SDL_Event native = new(new SDL_WindowEvent(SDL_EventType.WindowResized, 640, 360));
        SizeI expected = WindowScale.ToLogical(new SizeI(640, 360), engine.Window.ContentScale);

        engine.Receive(in native);

        _game.OnEvent(received => received is WindowResized { Size: var size } && size == expected).WasCalled(Times.Once);
    }

    [Test]
    public async Task DisposeShouldStopTheNativeSession()
    {
        KhaosEngine engine = TestEngine.Start(_game);

        engine.Dispose();

        await SDL3.WasInit(SDL_Subsystems.None).Should().BeEqualTo(SDL_Subsystems.None);
    }

    [Test]
    public void DisposeShouldLetAnotherGameStart()
    {
        KhaosEngine engine = TestEngine.Start(_game);
        engine.Dispose();

        Mock<Game2D> next = Game2D.Mock("next", 1280, 720);

        using KhaosEngine _ = TestEngine.Start(next);

        next.Initialize().WasCalled(Times.Once);
    }

    [Test]
    public async Task DisposeShouldNotStopTheEngineStartedAfterIt()
    {
        KhaosEngine first = TestEngine.Start(Game2D.Mock("first", 10, 10));
        first.Dispose();

        using KhaosEngine _ = TestEngine.Start(_game);

        first.Dispose();

        await NativeSession.Started.Should().BeTrue();
    }

    [Test]
    public async Task StartShouldGiveTheNameOfTheGameToTheSystem()
    {
        AppContext.SetData(GameMetadata.Name, "Duck Quest Deluxe");

        using KhaosEngine _ = TestEngine.Start(_game);

        string? name = SDLAppMetadata.Get(SDLAppMetadata.Name);

        await name.Should().BeEqualTo("Duck Quest Deluxe");
    }

    [Test]
    public async Task WindowShouldHaveTheTitleOfTheOptions()
    {
        WindowOptions options = new() { Title = "Duck Quest" };

        using KhaosEngine engine = TestEngine.Start(_game, options);

        await engine.Window.Title.Should().BeEqualTo("Duck Quest");
    }

    [Test]
    public async Task WindowShouldKeepTheTitleOfTheOptionsOverTheNameOfTheGame()
    {
        AppContext.SetData(GameMetadata.Name, "Duck Quest Deluxe");
        WindowOptions options = new() { Title = "Duck Quest Not Deluxe" };

        using KhaosEngine engine = TestEngine.Start(_game, options);

        await engine.Window.Title.Should().BeEqualTo("Duck Quest Not Deluxe");
    }

    [Test]
    public async Task WindowShouldHaveTheNameOfTheGameWhenTheOptionsHaveNoTitle()
    {
        AppContext.SetData(GameMetadata.Name, "Duck Quest Deluxe");

        using KhaosEngine engine = TestEngine.Start(_game, new WindowOptions());

        await engine.Window.Title.Should().BeEqualTo("Duck Quest Deluxe");
    }

    [Test]
    public async Task WindowShouldHaveNoTitleWhenTheOptionsHaveNoTitleAndTheGameNoName()
    {
        using KhaosEngine engine = TestEngine.Start(_game, new WindowOptions());

        await engine.Window.Title.Should().BeEmpty();
    }

    [Test]
    public async Task StartShouldGiveTheWindowToTheGame()
    {
        Window window = new();

        using KhaosEngine _ = TestEngine.Start(_game, window, null);

        await _game.Object.Window.Should().BeSameReferenceAs(window);
    }

    [Test]
    public async Task StartShouldGiveTheWindowToTheGameBeforeInitialize()
    {
        Window window = new();
        Window? seen = null;
        _game.Initialize().Callback(() => seen = _game.Object.Window);

        using KhaosEngine _ = TestEngine.Start(_game, window, null);

        await seen.Should().BeSameReferenceAs(window);
    }

    [Test]
    public void StartShouldDisposeTheServicesWhenTheEngineCannotStart()
    {
        IDisposableMock services = IDisposable.Mock();
        using KhaosEngine running = TestEngine.Start(Game2D.Mock(30, 30));

        _ = Assert.Throws<InvalidOperationException>(() => TestEngine.Start(_game, new Window(), services.Object));

        services.Dispose().WasCalled();
    }

    [Test]
    public void StartShouldDisposeTheServicesWhenInitializeThrows()
    {
        IDisposableMock services = IDisposable.Mock();
        _game.Initialize().Throws(new InvalidOperationException("The level file is missing."));

        _ = Assert.Throws<InvalidOperationException>(() => TestEngine.Start(_game, new Window(), services.Object));

        services.Dispose().WasCalled();
    }

    [Test]
    public void DisposeShouldDisposeTheServices()
    {
        IDisposableMock services = IDisposable.Mock();
        KhaosEngine engine = TestEngine.Start(_game, new Window(), services.Object);

        engine.Dispose();

        services.Dispose().WasCalled();
    }

    [Test]
    public async Task DisposeShouldDisposeTheServicesWhileTheWindowIsOpen()
    {
        Window window = new();
        string? titleSeen = null;
        IDisposableMock services = IDisposable.Mock();
        services.Dispose().Callback(() => titleSeen = window.Title);
        KhaosEngine engine = TestEngine.Start(_game, window, services.Object);

        engine.Dispose();

        await titleSeen.Should().BeEqualTo("Test");
    }

    [Test]
    public async Task DisposeShouldStopTheNativeSessionWhenTheServicesThrow()
    {
        IDisposableMock services = IDisposable.Mock();
        services.Dispose().Throws(new InvalidOperationException("A save file is locked."));
        KhaosEngine engine = TestEngine.Start(_game, new Window(), services.Object);

        _ = Assert.Throws<InvalidOperationException>(engine.Dispose);

        await NativeSession.Started.Should().BeFalse();
    }

    [Test]
    public void DisposeShouldCallShutdownOnce()
    {
        KhaosEngine engine = TestEngine.Start(_game);

        engine.Dispose();
        engine.Dispose();

        _game.Shutdown().WasCalled(Times.Once);
    }

    [Test]
    public async Task DisposeShouldCallShutdownWhileTheWindowIsOpen()
    {
        string? titleSeen = null;
        _game.Shutdown().Callback(() => titleSeen = _game.Object.Window.Title);
        KhaosEngine engine = TestEngine.Start(_game);

        engine.Dispose();

        await titleSeen.Should().BeEqualTo("Test");
    }

    [Test]
    public async Task DisposeShouldCallShutdownBeforeDisposingTheServices()
    {
        bool disposed = false;
        bool disposedBeforeShutdown = true;
        IDisposableMock services = IDisposable.Mock();
        services.Dispose().Callback(() => disposed = true);
        _game.Shutdown().Callback(() => disposedBeforeShutdown = disposed);
        KhaosEngine engine = TestEngine.Start(_game, new Window(), services.Object);

        engine.Dispose();

        await disposedBeforeShutdown.Should().BeFalse();
    }

    [Test]
    public void StartShouldNotCallShutdownWhenInitializeThrows()
    {
        _game.Initialize().Throws(new InvalidOperationException("The level file is missing."));

        _ = Assert.Throws<InvalidOperationException>(() => TestEngine.Start(_game));

        _game.Shutdown().WasNeverCalled();
    }

    [Test]
    public void DisposeShouldDisposeTheServicesWhenShutdownThrows()
    {
        IDisposableMock services = IDisposable.Mock();
        _game.Shutdown().Throws(new InvalidOperationException("The save file is locked."));
        KhaosEngine engine = TestEngine.Start(_game, new Window(), services.Object);

        _ = Assert.Throws<InvalidOperationException>(engine.Dispose);

        services.Dispose().WasCalled();
    }

    [Test]
    public async Task DisposeShouldStopTheNativeSessionWhenShutdownThrows()
    {
        _game.Shutdown().Throws(new InvalidOperationException("The save file is locked."));
        KhaosEngine engine = TestEngine.Start(_game);

        _ = Assert.Throws<InvalidOperationException>(engine.Dispose);

        await NativeSession.Started.Should().BeFalse();
    }

    [Test]
    public async Task DisposeShouldCloseTheWindow()
    {
        Window window = new();
        KhaosEngine engine = TestEngine.Start(_game, window, null);

        engine.Dispose();

        await Assert.That(() => window.Title).Throws<InvalidOperationException>();
    }
}
