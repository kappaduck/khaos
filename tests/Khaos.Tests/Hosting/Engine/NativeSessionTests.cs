// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting.Engine;
using KappaDuck.Khaos.Interop.SDL;
using KappaDuck.Khaos.Interop.SDL.Primitives;
using TUnit.Core.Enums;

namespace KappaDuck.Khaos.Tests.Hosting.Engine;

[Category(Categories.Integration)]
[NotInParallel(Sequential.Engine)]
internal sealed class NativeSessionTests
{
    private const string AllowScreenSaver = "SDL_VIDEO_ALLOW_SCREENSAVER";
    private const SDL_Subsystems InitializedSubsystems = SDL_Subsystems.Video | SDL_Subsystems.Events;
    private const string MissingDriver = "khaos-missing";

    private static readonly AppMetadata _metadata = new()
    {
        Name = "Duck Quest",
        Version = "1.0.0",
        Identifier = "com.kappaduck.duck.quest",
        Creator = "KappaDuck",
        Copyright = "Copyright (c) KappaDuck",
        Url = new Uri("https://github.com/kappaduck/duck-quest")
    };

    [After(Test)]
    public void ForgetEnvironment()
    {
        SDLEnvironment.Unset(AllowScreenSaver);
        SDLEnvironment.Unset(SDLHints.NoSignalHandlers);
    }

    [Test]
    public async Task StartShouldApplyANativeSettingBeforeInit()
    {
        using NativeSession _ = NativeSession.Start(_metadata, Native(AllowScreenSaver, "1"));

        await SDLVideo.ScreenSaverEnabled().Should().BeTrue();
    }

    [Test]
    public async Task StartShouldLetAnEnvironmentVariableWinOverANativeSetting()
    {
        SDLEnvironment.Set(AllowScreenSaver, "0");

        using NativeSession _ = NativeSession.Start(_metadata, Native(AllowScreenSaver, "1"));

        await SDLVideo.ScreenSaverEnabled().Should().BeFalse();
    }

    [Test]
    public async Task StartShouldKeepTheSignalHandlersOfSDLOffWhateverTheEnvironmentSays()
    {
        SDLEnvironment.Set(SDLHints.NoSignalHandlers, "0");

        using NativeSession _ = NativeSession.Start(_metadata);

        await SDLHints.Get(SDLHints.NoSignalHandlers).Should().BeEqualTo("1");
    }

    [Test]
    [RealScreenOnly]
    [RunOn(OS.Windows)]
    public async Task StartShouldFallBackWhenTheVideoDriverIsMissing()
    {
        using NativeSession _ = NativeSession.Start(_metadata, new EngineOptions { VideoDriver = VideoDriver.Wayland });

        await SDLVideo.GetCurrentVideoDriver().Should().BeEqualTo("windows");
    }

    [Test]
    [OffscreenOnly]
    public async Task StartShouldLetAnEnvironmentVariableWinOverTheVideoDriver()
    {
        using NativeSession _ = NativeSession.Start(_metadata, new EngineOptions { VideoDriver = VideoDriver.X11 });

        await SDLVideo.GetCurrentVideoDriver().Should().BeEqualTo("offscreen");
    }

    [Test]
    [RealScreenOnly]
    public async Task StartShouldUseTheVideoDriver()
    {
        using NativeSession _ = NativeSession.Start(_metadata, new EngineOptions { VideoDriver = VideoDriver.Offscreen });

        await SDLVideo.GetCurrentVideoDriver().Should().BeEqualTo("offscreen");
    }

    [Test]
    public async Task StartShouldInitializeVideo()
    {
        using NativeSession _ = NativeSession.Start(_metadata);

        SDL_Subsystems initialized = SDL3.WasInit(InitializedSubsystems);

        await initialized.Should().BeEqualTo(InitializedSubsystems);
    }

    [Test]
    public async Task StartShouldSetTheAppMetadata()
    {
        using NativeSession _ = NativeSession.Start(_metadata);

        string? name = SDLAppMetadata.Get(SDLAppMetadata.Name);
        string? version = SDLAppMetadata.Get(SDLAppMetadata.Version);
        string? identifier = SDLAppMetadata.Get(SDLAppMetadata.Identifier);
        string? creator = SDLAppMetadata.Get(SDLAppMetadata.Creator);
        string? copyright = SDLAppMetadata.Get(SDLAppMetadata.Copyright);
        string? url = SDLAppMetadata.Get(SDLAppMetadata.Url);
        string? type = SDLAppMetadata.Get(SDLAppMetadata.Type);

        await name.Should().BeEqualTo(_metadata.Name);
        await version.Should().BeEqualTo(_metadata.Version);
        await identifier.Should().BeEqualTo(_metadata.Identifier);
        await creator.Should().BeEqualTo(_metadata.Creator);
        await copyright.Should().BeEqualTo(_metadata.Copyright);
        await url.Should().BeEqualTo("https://github.com/kappaduck/duck-quest");
        await type.Should().BeEqualTo("game");
    }

    [Test]
    public async Task StartShouldTurnOffTheSignalHandlersOfSDL()
    {
        using NativeSession _ = NativeSession.Start(_metadata);

        string? hint = SDLHints.Get(SDLHints.NoSignalHandlers);

        await hint.Should().BeEqualTo("1");
    }

    [Test]
    public async Task StartShouldLeaveUnsetMetadataEmpty()
    {
        using NativeSession _ = NativeSession.Start(_metadata with { Version = null });

        string? version = SDLAppMetadata.Get(SDLAppMetadata.Version);

        await version.Should().BeNull();
    }

    [Test]
    public async Task StartShouldThrowWhenTheNativeSessionIsAlreadyStarted()
    {
        using NativeSession _ = NativeSession.Start(_metadata);

        Action startAgain = static () =>
        {
            using NativeSession _ = NativeSession.Start(_metadata);
        };

        await startAgain.Should().ThrowExactly<InvalidOperationException>();
    }

    [Test]
    public async Task StartShouldThrowAPlatformExceptionWhenTheVideoDriverIsMissing()
    {
        SDLHints.UseVideoDriver(MissingDriver);

        PlatformException exception = Assert.Throws<PlatformException>(static () =>
        {
            using NativeSession _ = NativeSession.Start(_metadata);
        });

        await exception.Message.Should().EndWith($" failed: {MissingDriver} not available");
    }

    [Test]
    public async Task StartShouldLeaveNothingInitializedWhenInitFails()
    {
        SDLHints.UseVideoDriver(MissingDriver);

        _ = Assert.Throws<PlatformException>(static () =>
        {
            using NativeSession _ = NativeSession.Start(_metadata);
        });

        SDL_Subsystems initialized = SDL3.WasInit(SDL_Subsystems.None);

        await initialized.Should().BeEqualTo(SDL_Subsystems.None);
    }

    [Test]
    public async Task StartShouldSucceedAfterAFailedStart()
    {
        SDLHints.UseVideoDriver(MissingDriver);

        _ = Assert.Throws<PlatformException>(static () =>
        {
            using NativeSession _ = NativeSession.Start(_metadata);
        });

        using NativeSession session = NativeSession.Start(_metadata);

        SDL_Subsystems initialized = SDL3.WasInit(InitializedSubsystems);

        await initialized.Should().BeEqualTo(InitializedSubsystems);
    }

    [Test]
    public async Task DisposeShouldQuitEverySubsystem()
    {
        NativeSession session = NativeSession.Start(_metadata);

        session.Dispose();

        SDL_Subsystems started = SDL3.WasInit(SDL_Subsystems.None);
        await started.Should().BeEqualTo(SDL_Subsystems.None);
    }

    [Test]
    public async Task DisposeShouldBeSafeToCallTwice()
    {
        NativeSession session = NativeSession.Start(_metadata);
        session.Dispose();

        await Assert.That(session.Dispose).ThrowsNothing();
    }

    [Test]
    public async Task DisposeShouldNotStopANativeSessionStartedAfterIt()
    {
        NativeSession first = NativeSession.Start(_metadata);
        first.Dispose();

        using NativeSession second = NativeSession.Start(_metadata);
        first.Dispose();

        SDL_Subsystems initialized = SDL3.WasInit(InitializedSubsystems);
        await initialized.Should().BeEqualTo(InitializedSubsystems);
    }

    [Test]
    public async Task StartedShouldBeTrueWhileASessionIsStarted()
    {
        using NativeSession _ = NativeSession.Start(_metadata);

        await NativeSession.Started.Should().BeTrue();
    }

    [Test]
    public async Task IsStartedShouldBeFalseOnceTheSessionIsDisposed()
    {
        NativeSession session = NativeSession.Start(_metadata);

        session.Dispose();

        await NativeSession.Started.Should().BeFalse();
    }

    private static EngineOptions Native(string name, string value)
    {
        EngineOptions engine = new();
        engine.Native[name] = value;

        return engine;
    }
}
