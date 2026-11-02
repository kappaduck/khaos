// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting;
using KappaDuck.Khaos.Interop.SDL;
using KappaDuck.Khaos.Interop.SDL.Primitives;
using KappaDuck.Khaos.Tests.Helpers;

namespace KappaDuck.Khaos.Tests.Hosting;

[NotInParallel("NativeLifecycle")]
internal sealed class EngineHostTests
{
    private const SDL_Subsystems InitializedSubsystems = SDL_Subsystems.Video | SDL_Subsystems.Events;
    private const string MissingDriver = "khaos-missing";

    private static readonly AppMetadata _game = new() { Name = "Duck Quest" };

    private static readonly AppMetadata _completeGame = new()
    {
        Name = "Duck Quest",
        Version = "1.0.0",
        Identifier = "com.kappaduck.duck.quest",
        Creator = "KappaDuck",
        Copyright = "Copyright (c) KappaDuck",
        Url = new Uri("https://github.com/kappaduck/duck-quest")
    };

    [Test]
    public async Task StartShouldInitializeVideoAndEvents()
    {
        using EngineHost _ = EngineHost.Start(_game);

        SDL_Subsystems started = SDL3.WasInit(InitializedSubsystems);

        await started.Should().BeEqualTo(InitializedSubsystems);
    }

    [Test]
    [NamedVideoDriverOnly]
    public async Task StartShouldUseTheVideoDriverOfTheSession()
    {
        using EngineHost _ = EngineHost.Start(_game);

        string? driver = SDLVideo.GetCurrentVideoDriver();

        await driver.Should().BeEqualTo(VideoDriverSetup.Driver);
    }

    [Test]
    public async Task StartShouldSetTheAppMetadata()
    {
        using EngineHost _ = EngineHost.Start(_completeGame);

        string? name = SDLAppMetadata.Get(SDLAppMetadata.Name);
        string? version = SDLAppMetadata.Get(SDLAppMetadata.Version);
        string? identifier = SDLAppMetadata.Get(SDLAppMetadata.Identifier);
        string? creator = SDLAppMetadata.Get(SDLAppMetadata.Creator);
        string? copyright = SDLAppMetadata.Get(SDLAppMetadata.Copyright);
        string? url = SDLAppMetadata.Get(SDLAppMetadata.Url);
        string? type = SDLAppMetadata.Get(SDLAppMetadata.Type);

        await name.Should().BeEqualTo(_completeGame.Name);
        await version.Should().BeEqualTo(_completeGame.Version);
        await identifier.Should().BeEqualTo(_completeGame.Identifier);
        await creator.Should().BeEqualTo(_completeGame.Creator);
        await copyright.Should().BeEqualTo(_completeGame.Copyright);
        await url.Should().BeEqualTo("https://github.com/kappaduck/duck-quest");
        await type.Should().BeEqualTo("game");
    }

    [Test]
    public async Task StartShouldLeaveUnsetMetadataEmpty()
    {
        using EngineHost _ = EngineHost.Start(_game);

        string? version = SDLAppMetadata.Get(SDLAppMetadata.Version);

        await version.Should().BeNull();
    }

    [Test]
    public async Task StartShouldThrowWhenTheEngineHostIsAlreadyStarted()
    {
        using EngineHost _ = EngineHost.Start(_game);

        Action startAgain = static () =>
        {
            using EngineHost _ = EngineHost.Start(_game);
        };

        await startAgain.Should().ThrowExactly<InvalidOperationException>();
    }

    [Test]
    public async Task StartShouldThrowAPlatformExceptionWhenTheVideoDriverIsMissing()
    {
        SDLHints.ForceVideoDriver(MissingDriver);

        PlatformException exception = Assert.Throws<PlatformException>(static () =>
        {
            using EngineHost _ = EngineHost.Start(_game);
        });

        await exception.Message.Should().EndWith($" failed: {MissingDriver} not available");
    }

    [Test]
    public async Task StartShouldLeaveNothingInitializedWhenInitFails()
    {
        SDLHints.ForceVideoDriver(MissingDriver);
        _ = Assert.Throws<PlatformException>(static () =>
        {
            using EngineHost _ = EngineHost.Start(_game);
        });

        SDL_Subsystems started = SDL3.WasInit(SDL_Subsystems.None);

        await started.Should().BeEqualTo(SDL_Subsystems.None);
    }

    [Test]
    public async Task StartShouldSucceedAfterAFailedStart()
    {
        SDLHints.ForceVideoDriver(MissingDriver);
        _ = Assert.Throws<PlatformException>(static () =>
        {
            using EngineHost _ = EngineHost.Start(_game);
        });

        using EngineHost engine = EngineHost.Start(_game);

        SDL_Subsystems started = SDL3.WasInit(InitializedSubsystems);
        await started.Should().BeEqualTo(InitializedSubsystems);
    }

    [Test]
    public async Task DisposeShouldQuitEverySubsystem()
    {
        EngineHost engine = EngineHost.Start(_game);

        engine.Dispose();

        SDL_Subsystems started = SDL3.WasInit(SDL_Subsystems.None);
        await started.Should().BeEqualTo(SDL_Subsystems.None);
    }

    [Test]
    public async Task DisposeShouldBeSafeToCallTwice()
    {
        EngineHost engine = EngineHost.Start(_game);
        engine.Dispose();

        await Assert.That(engine.Dispose).ThrowsNothing();
    }

    [Test]
    public async Task DisposeShouldNotStopAnEngineHostStartedAfterIt()
    {
        EngineHost first = EngineHost.Start(_game);
        first.Dispose();

        using EngineHost second = EngineHost.Start(_game);
        first.Dispose();

        SDL_Subsystems started = SDL3.WasInit(InitializedSubsystems);
        await started.Should().BeEqualTo(InitializedSubsystems);
    }
}
