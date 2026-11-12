// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Graphics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KappaDuck.Khaos.Tests.Graphics;

internal sealed class GraphicsOptionsTests
{
    [Test]
    public async Task ValueShouldTurnVSyncOnByDefault()
    {
        GraphicsOptions options = Resolve(Configuration([]), static _ => { });

        await options.VSync.Should().BeEqualTo(VSync.Enabled);
    }

    [Test]
    public async Task ConfigureShouldGiveTheValueWhenTheSectionIsEmpty()
    {
        GraphicsOptions options = Resolve(Configuration([]), static graphics => graphics.VSync = VSync.Adaptive);

        await options.VSync.Should().BeEqualTo(VSync.Adaptive);
    }

    [Test]
    public async Task PostConfigureShouldLetConfigurationReplaceTheCode()
    {
        IConfiguration configuration = Configuration(new() { ["graphics:vsync"] = "false" });

        GraphicsOptions options = Resolve(configuration, static graphics => graphics.VSync = VSync.Adaptive);

        await options.VSync.Should().BeEqualTo(VSync.Disabled);
    }

    [Test]
    public async Task PostConfigureShouldReadANumberOfRefreshes()
    {
        IConfiguration configuration = Configuration(new() { ["graphics:vsync"] = "2" });

        GraphicsOptions options = Resolve(configuration, static _ => { });

        await options.VSync.Should().BeEqualTo(VSync.Every(2));
    }

    [Test]
    public async Task PostConfigureShouldReadKeysWhateverTheirCase()
    {
        IConfiguration configuration = Configuration(new() { ["Graphics:VSync"] = "Adaptive" });

        GraphicsOptions options = Resolve(configuration, static _ => { });

        await options.VSync.Should().BeEqualTo(VSync.Adaptive);
    }

    [Test]
    public async Task PostConfigureShouldReadVSyncFromTheCommandLine()
    {
        IConfiguration configuration = new ConfigurationBuilder().AddCommandLine(["--graphics:vsync", "false"]).Build();

        GraphicsOptions options = Resolve(configuration, static _ => { });

        await options.VSync.Should().BeEqualTo(VSync.Disabled);
    }

    [Test]
    public async Task ValueShouldNotLimitTheFrameRateByDefault()
    {
        GraphicsOptions options = Resolve(Configuration([]), static _ => { });

        await options.MaxFrameRate.Should().BeEqualTo(0);
    }

    [Test]
    public async Task PostConfigureShouldReadMaxFrameRate()
    {
        IConfiguration configuration = Configuration(new() { ["graphics:maxFrameRate"] = "60" });

        GraphicsOptions options = Resolve(configuration, static graphics => graphics.MaxFrameRate = 30);

        await options.MaxFrameRate.Should().BeEqualTo(60);
    }

    [Test]
    public async Task ValueShouldThrowWhenMaxFrameRateIsNegativeInTheSettings()
    {
        IConfiguration configuration = Configuration(new() { ["graphics:maxFrameRate"] = "-5" });

        KhaosException exception = Assert.Throws<KhaosException>(() => Resolve(configuration, static _ => { }));

        await exception.Message.Should().Contain("graphics:maxFrameRate");
    }

    [Test]
    public async Task ValueShouldThrowWhenMaxFrameRateIsNegativeInTheCode()
    {
        KhaosException exception = Assert.Throws<KhaosException>(() => Resolve(Configuration([]), static graphics => graphics.MaxFrameRate = -1));

        await exception.Message.Should().Contain("-1");
    }

    [Test]
    public async Task ValueShouldThrowWhenVSyncIsUnknown()
    {
        IConfiguration configuration = Configuration(new() { ["graphics:vsync"] = "maybe" });

        KhaosException exception = Assert.Throws<KhaosException>(() => Resolve(configuration, static _ => { }));

        await exception.Message.Should().Contain("graphics:vsync");
        await exception.Message.Should().Contain("maybe");
    }

    private static IConfiguration Configuration(Dictionary<string, string?> values)
    => new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    private static GraphicsOptions Resolve(IConfiguration configuration, Action<GraphicsOptions> code)
    {
        ServiceCollection services = new();

        services.Configure(code);
        services.BindGraphicsSettings(configuration);

        return services.BuildServiceProvider().GetRequiredService<IOptions<GraphicsOptions>>().Value;
    }
}
