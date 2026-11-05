// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Graphics.Rendering;
using KappaDuck.Khaos.Hosting;
using KappaDuck.Khaos.Tests.Helpers;
using KappaDuck.Khaos.Windowing;
using Microsoft.Extensions.Logging;
using TUnit.Mocks.Logging;

namespace KappaDuck.Khaos.Tests.Graphics.Rendering;

[Category(Categories.Integration)]
[NotInParallel(Sequential.Engine)]
internal sealed class RendererTests
{
    private readonly MockLogger<Renderer> _logger = Mock.Logger<Renderer>();
    private readonly WindowOptions _options = new() { Title = "Duck Quest", Width = 640, Height = 360 };

    private EngineHost _host = null!;
    private Window _window = null!;

    private Renderer _renderer = null!;

    [Before(Test)]
    public void StartEngine()
    {
        _host = EngineHost.Start(new AppMetadata() { Name = "Duck Quest" });
        _window = Window.Open(_options);

        _renderer = Renderer.Create(_window, VSync.Disabled, _logger);
    }

    [After(Test)]
    public void StopEngine()
    {
        _renderer.Destroy();
        _window.Close();

        _host.Dispose();
    }

    [Test]
    public async Task ClearShouldFillTheWindowWithTheColor()
    {
        _renderer.Clear(Color.Teal);

        await SDLRender.ReadPixel(_renderer, 0, 0).Should().BeEqualTo(Color.Teal);
        await SDLRender.ReadPixel(_renderer, 639, 359).Should().BeEqualTo(Color.Teal);
    }

    [Test]
    public async Task ClearShouldReplaceThePreviousColor()
    {
        _renderer.Clear(Color.Crimson);
        _renderer.Clear(Color.DarkSlateGray);

        await SDLRender.ReadPixel(_renderer, 320, 180).Should().BeEqualTo(Color.DarkSlateGray);
    }

    [Test]
    public async Task PresentShouldShowTheFrame()
    {
        _renderer.Clear(Color.MidnightBlue);
        _renderer.Present();

        await Assert.That(_renderer.Present).ThrowsNothing();
    }

    [Test]
    public async Task CreateShouldTurnVSyncOff()
    {
        using WindowScope scope = WindowScope.Open(_options);
        Renderer renderer = Renderer.Create(scope.Window, VSync.Disabled, _logger);

        int vsync = SDLRender.ReadVSync(renderer);

        await vsync.Should().BeEqualTo(0);

        renderer.Destroy();
    }

    [Test]
    public async Task CreateShouldTurnVSyncOn()
    {
        using WindowScope scope = WindowScope.Open(_options);
        Renderer renderer = Renderer.Create(scope.Window, VSync.Enabled, _logger);

        int vsync = SDLRender.ReadVSync(renderer);

        await vsync.Should().BeEqualTo(1);

        renderer.Destroy();
    }

    [Test]
    public void CreateShouldNotLogWhenTheDriverAcceptsVSync()
    {
        using WindowScope scope = WindowScope.Open(_options);
        Renderer renderer = Renderer.Create(scope.Window, VSync.Disabled, _logger);

        _logger.VerifyNoLogs();

        renderer.Destroy();
    }

    [Test]
    [OffscreenOnly]
    public async Task CreateShouldWaitForEveryRefreshWhenAdaptiveIsRefused()
    {
        using WindowScope scope = WindowScope.Open(_options);
        Renderer renderer = Renderer.Create(scope.Window, VSync.Adaptive, _logger);

        int vsync = SDLRender.ReadVSync(renderer);

        await vsync.Should().BeEqualTo(1);

        renderer.Destroy();
    }

    [Test]
    [OffscreenOnly]
    public async Task CreateShouldWaitForEveryRefreshWhenSeveralRefreshesAreRefused()
    {
        using WindowScope scope = WindowScope.Open(_options);
        Renderer renderer = Renderer.Create(scope.Window, VSync.Every(3), _logger);

        int vsync = SDLRender.ReadVSync(renderer);

        await vsync.Should().BeEqualTo(1);

        renderer.Destroy();
    }

    [Test]
    [OffscreenOnly]
    public void CreateShouldLogTheRefusedVSyncAsInformation()
    {
        using WindowScope scope = WindowScope.Open(_options);
        Renderer renderer = Renderer.Create(scope.Window, VSync.Adaptive, _logger);

        _logger.VerifyLog()
            .AtLevel(LogLevel.Information)
            .ContainingMessage("Adaptive")
            .ContainingMessage("every refresh");

        renderer.Destroy();
    }

    [Test]
    public async Task ClearShouldThrowOnceTheRendererIsDestroyed()
    {
        using WindowScope scope = WindowScope.Open(_options);
        Renderer renderer = Renderer.Create(scope.Window, VSync.Disabled, _logger);

        renderer.Destroy();

        await Assert.That(() => renderer.Clear(Color.Black)).Throws<ObjectDisposedException>();
    }

    [Test]
    public async Task DestroyShouldDoNothingTheSecondTime()
    {
        using WindowScope scope = WindowScope.Open(_options);
        Renderer renderer = Renderer.Create(scope.Window, VSync.Disabled, _logger);

        renderer.Destroy();

        await Assert.That(renderer.Destroy).ThrowsNothing();
    }

    [Test]
    public async Task CreateShouldRejectAClosedWindow()
    {
        _window.Close();

        await Assert.That(() => Renderer.Create(_window, VSync.Disabled, _logger)).Throws<ObjectDisposedException>();
    }
}
