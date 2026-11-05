// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;
using KappaDuck.Khaos.Windowing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KappaDuck.Khaos.Tests.Windowing;

internal sealed class WindowOptionsTests
{
    [Test]
    public async Task ValueShouldOpenAt1280By720ByDefault()
    {
        WindowOptions options = Resolve(Configuration([]), static _ => { });

        await options.Width.Should().BeEqualTo(1280);
        await options.Height.Should().BeEqualTo(720);
    }

    [Test]
    public async Task ConfigureShouldGiveTheValuesWhenTheSectionIsEmpty()
    {
        WindowOptions options = Resolve(Configuration([]), static window => window.Title = "Ball");

        await options.Title.Should().BeEqualTo("Ball");
    }

    [Test]
    public async Task PostConfigureShouldLetConfigurationReplaceTheCode()
    {
        IConfiguration configuration = Configuration(new() { ["window:title"] = "Pong", ["window:width"] = "640" });

        WindowOptions options = Resolve(configuration, static window =>
        {
            window.Title = "Ball";
            window.Width = 1920;
        });

        await options.Title.Should().BeEqualTo("Pong");
        await options.Width.Should().BeEqualTo(640);
    }

    [Test]
    public async Task PostConfigureShouldLetConfigurationReplaceTheHeight()
    {
        IConfiguration configuration = Configuration(new() { ["window:height"] = "480" });

        WindowOptions options = Resolve(configuration, static window => window.Height = 1080);

        await options.Height.Should().BeEqualTo(480);
    }

    [Test]
    public async Task PostConfigureShouldWinEvenWhenRegisteredBeforeTheCode()
    {
        IConfiguration configuration = Configuration(new() { ["window:resizable"] = "false" });
        ServiceCollection services = new();

        services.ConfigureWindow(configuration);
        services.Configure<WindowOptions>(static window => window.Resizable = true);

        WindowOptions options = services.BuildServiceProvider().GetRequiredService<IOptions<WindowOptions>>().Value;

        await options.Resizable.Should().BeFalse();
    }

    [Test]
    public async Task PostConfigureShouldReadKeysWhateverTheirCase()
    {
        IConfiguration configuration = Configuration(new() { ["Window:Resizable"] = "true", ["window:borderless"] = "true" });

        WindowOptions options = Resolve(configuration, static _ => { });

        await options.Resizable.Should().BeTrue();
        await options.Borderless.Should().BeTrue();
    }

    [Test]
    public async Task PostConfigureShouldReadFullscreenFromTheCommandLine()
    {
        IConfiguration configuration = new ConfigurationBuilder().AddCommandLine(["--window:fullscreen", "true"]).Build();

        WindowOptions options = Resolve(configuration, static _ => { });

        await options.Fullscreen.Should().BeTrue();
    }

    [Test]
    public async Task ValueShouldOpenOnThePrimaryDisplayByDefault()
    {
        WindowOptions options = Resolve(Configuration([]), static _ => { });

        await options.Display.Should().BeNull();
    }

    [Test]
    public async Task PostConfigureShouldIgnoreADisplayInTheSettings()
    {
        IConfiguration configuration = Configuration(new() { ["window:display"] = "U28E590", ["window:display:id"] = "1" });

        WindowOptions options = Resolve(configuration, static _ => { });

        await options.Display.Should().BeNull();
    }

    [Test]
    public async Task ValueShouldCenterTheWindowByDefault()
    {
        WindowOptions options = Resolve(Configuration([]), static _ => { });

        await options.Position.Should().BeNull();
    }

    [Test]
    public async Task PostConfigureShouldIgnoreAPositionInTheSettings()
    {
        IConfiguration configuration = Configuration(new()
        {
            ["window:position:x"] = "120",
            ["window:position:y"] = "-80",
            ["window:x"] = "120",
            ["window:y"] = "-80"
        });

        WindowOptions options = Resolve(configuration, static _ => { });

        await options.Position.Should().BeNull();
    }

    [Test]
    public async Task PostConfigureShouldKeepThePositionTheCodeChose()
    {
        IConfiguration configuration = Configuration(new() { ["window:position:x"] = "120", ["window:position:y"] = "-80" });

        WindowOptions options = Resolve(configuration, static window => window.Position = new PointI(40, 30));

        await options.Position.Should().BeEqualTo(new PointI(40, 30));
    }

    [Test]
    public async Task ValueShouldThrowWhenTheWidthIsNotPositive()
    {
        IConfiguration configuration = Configuration(new() { ["window:width"] = "0" });

        KhaosException exception = Assert.Throws<KhaosException>(() => Resolve(configuration, static _ => { }));

        await exception.Message.Should().Contain("window:width");
    }

    [Test]
    public async Task ValueShouldThrowWhenTheHeightIsNotANumber()
    {
        IConfiguration configuration = Configuration(new() { ["window:height"] = "tall" });

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => Resolve(configuration, static _ => { }));

        await exception.Message.ToLowerInvariant().Should().Contain("window:height");
    }

    private static IConfiguration Configuration(Dictionary<string, string?> values)
        => new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    private static WindowOptions Resolve(IConfiguration configuration, Action<WindowOptions> code)
    {
        ServiceCollection services = new();

        services.Configure(code);
        services.ConfigureWindow(configuration);

        return services.BuildServiceProvider().GetRequiredService<IOptions<WindowOptions>>().Value;
    }
}
