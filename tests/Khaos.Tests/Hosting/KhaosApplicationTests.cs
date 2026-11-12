// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Hosting;
using KappaDuck.Khaos.Hosting.Engine;
using KappaDuck.Khaos.Windowing;
using Microsoft.Extensions.DependencyInjection;

namespace KappaDuck.Khaos.Tests.Hosting;

[Category(Categories.Integration)]
[NotInParallel(Sequential.Engine)]
internal sealed class KhaosApplicationTests
{
    private readonly TestHost _host = new();

    [Before(Test)]
    public void ForgetEnvironment() => TestHost.ForgetEnvironment();

    [After(Test)]
    public void Clean() => _host.Delete();

    [Test]
    public async Task BuildShouldThrowWhenCalledTwice()
    {
        KhaosApplicationBuilder builder = _host.Create();
        _ = builder.Build<ProbedGame>();

        await Assert.That(() => builder.Build<ProbedGame>()).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task BuildShouldThrowWhenAServiceOfTheGameIsMissing()
    {
        KhaosApplicationBuilder builder = KhaosApplication.CreateBuilder();

        await Assert.That(() => builder.Build<ProbedGame>()).Throws<AggregateException>();
    }

    [Test]
    public async Task RunShouldReturnZeroWhenTheGameExits()
    {
        KhaosApplication application = _host.Create().Build<ProbedGame>();

        int exitCode = application.Run();

        await exitCode.Should().BeZero();
    }

    [Test]
    public async Task RunShouldThrowWhenCalledTwice()
    {
        KhaosApplication application = _host.Create().Build<ProbedGame>();
        _ = application.Run();

        await Assert.That(application.Run).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task RunShouldCreateTheGameWithItsServices()
    {
        KhaosApplication application = _host.Create().Build<ProbedGame>();

        _ = application.Run();

        await _host.Probe.Created.Should().BeTrue();
    }

    [Test]
    public async Task RunShouldGiveTheGameTheWindowInjectedIntoItsServices()
    {
        KhaosApplicationBuilder builder = _host.Create();
        builder.Services.AddSingleton<Camera>();

        _ = builder.Build<ProbedGame>().Run();

        await _host.Probe.GameWindow.Should().BeSameReferenceAs(_host.Probe.CameraWindow);
    }

    [Test]
    public async Task RunShouldOpenTheWindowWithTheTitleOfTheGame()
    {
        KhaosApplication application = _host.Create().Build<ProbedGame>();

        _ = application.Run();

        await _host.Probe.Title.Should().BeEqualTo("Hosted");
    }

    [Test]
    public async Task RunShouldPreferTheOptionsOfProgramOverTheGame()
    {
        KhaosApplicationBuilder builder = _host.Create();
        builder.Services.Configure<WindowOptions>(static options => options.Title = "From Program");

        _ = builder.Build<ProbedGame>().Run();

        await _host.Probe.Title.Should().BeEqualTo("From Program");
    }

    [Test]
    public async Task BuildShouldThrowWhenTheGameReadsTheWindowOptionsInItsConstructor()
    {
        KhaosApplicationBuilder builder = _host.Create();

        await Assert.That(builder.Build<OptionsReadingGame>).Throws<AggregateException>().WithMessageContaining("circular dependency");
    }

    [Test]
    public async Task RunShouldThrowAnInvalidOptionBeforeTheGameStarts()
    {
        KhaosApplicationBuilder builder = _host.Create();
        builder.Services.Configure<WindowOptions>(static options => options.Width = 0);
        KhaosApplication application = builder.Build<ProbedGame>();

        await Assert.That(application.Run).Throws<KhaosException>();
    }

    [Test]
    public async Task RunShouldNotOpenTheWindowWhenAnOptionIsInvalid()
    {
        KhaosApplicationBuilder builder = _host.Create();
        builder.Services.Configure<WindowOptions>(static options => options.Width = 0);

        _ = Assert.Throws<KhaosException>(() => builder.Build<ProbedGame>().Run());

        await _host.Probe.GameWindow.Should().BeNull();
    }

    [Test]
    public async Task RunShouldDisposeTheGameWhenAnOptionIsInvalid()
    {
        KhaosApplicationBuilder builder = _host.Create();
        builder.Services.Configure<WindowOptions>(static options => options.Width = 0);

        _ = Assert.Throws<KhaosException>(() => builder.Build<ProbedGame>().Run());

        await _host.Probe.Disposed.Should().BeTrue();
    }

    [Test]
    public async Task RunShouldNotCallShutdownWhenAnOptionIsInvalid()
    {
        KhaosApplicationBuilder builder = _host.Create();
        builder.Services.Configure<WindowOptions>(static options => options.Width = 0);

        _ = Assert.Throws<KhaosException>(() => builder.Build<ProbedGame>().Run());

        await _host.Probe.ShutDown.Should().BeFalse();
    }

    [Test]
    public async Task RunShouldCallShutdownWhileTheWindowIsOpen()
    {
        KhaosApplication application = _host.Create().Build<ProbedGame>();

        _ = application.Run();

        await _host.Probe.TitleAtShutdown.Should().BeEqualTo("Hosted");
    }

    [Test]
    public async Task RunShouldDisposeTheGameOnceItExits()
    {
        KhaosApplication application = _host.Create().Build<ProbedGame>();

        _ = application.Run();

        await _host.Probe.Disposed.Should().BeTrue();
    }

    [Test]
    public async Task RunShouldNotCreateTheGameWhenAnotherGameIsRunning()
    {
        KhaosApplication application = _host.Create().Build<ProbedGame>();
        using KhaosEngine running = TestEngine.Start(Game2D.Mock(30, 30));

        _ = Assert.Throws<InvalidOperationException>(() => application.Run());

        await _host.Probe.Created.Should().BeFalse();
    }

    [Test]
    public async Task ConfigureWindowShouldChangeTheWindow()
    {
        KhaosApplicationBuilder builder = _host.Create();
        builder.ConfigureWindow(static window => window.Title = "From ConfigureWindow");

        _ = builder.Build<ProbedGame>().Run();

        await _host.Probe.Title.Should().BeEqualTo("From ConfigureWindow");
    }

    [Test]
    public async Task ConfigureWindowShouldReturnTheBuilder()
    {
        KhaosApplicationBuilder builder = _host.Create();

        KhaosApplicationBuilder returned = builder.ConfigureWindow(static _ => { });

        await returned.Should().BeSameReferenceAs(builder);
    }

    [Test]
    public async Task ConfigureWindowShouldYieldToTheSettings()
    {
        KhaosApplicationBuilder builder = _host.Create("--window:title", "From the settings");
        builder.ConfigureWindow(static window => window.Title = "From ConfigureWindow");

        _ = builder.Build<ProbedGame>().Run();

        await _host.Probe.Title.Should().BeEqualTo("From the settings");
    }

    [Test]
    public async Task ConfigureGraphicsShouldChangeTheGraphics()
    {
        KhaosApplicationBuilder builder = _host.Create();
        builder.ConfigureGraphics(static graphics => graphics.VSync = VSync.Disabled);

        _ = builder.Build<ProbedGame>().Run();

        await _host.Probe.VSync.Should().BeEqualTo(0);
    }

    [Test]
    public async Task ConfigureGraphicsShouldReturnTheBuilder()
    {
        KhaosApplicationBuilder builder = _host.Create();

        KhaosApplicationBuilder returned = builder.ConfigureGraphics(static _ => { });

        await returned.Should().BeSameReferenceAs(builder);
    }

    [Test]
    public async Task RunShouldGiveTheRendererTheVSyncOfTheSettings()
    {
        KhaosApplication application = _host.Create("--graphics:vsync", "false").Build<ProbedGame>();

        _ = application.Run();

        await _host.Probe.VSync.Should().BeEqualTo(0);
    }
}
