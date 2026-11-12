// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KappaDuck.Khaos.Tests;

internal sealed class LoopOptionsTests
{
    [Test]
    public async Task ValueShouldRunSixtyStepsPerSecondByDefault()
    {
        LoopOptions options = Resolve(Configuration([]), static _ => { });

        await options.FixedRate.Should().BeEqualTo(60);
    }

    [Test]
    public async Task ValueShouldCapAFrameAtAQuarterOfASecondByDefault()
    {
        LoopOptions options = Resolve(Configuration([]), static _ => { });

        await options.MaxFrameTime.Should().BeEqualTo(0.25);
    }

    [Test]
    public async Task ConfigureShouldGiveTheValueWhenTheSectionIsEmpty()
    {
        LoopOptions options = Resolve(Configuration([]), static loop => loop.FixedRate = 120);

        await options.FixedRate.Should().BeEqualTo(120);
    }

    [Test]
    public async Task PostConfigureShouldLetConfigurationReplaceTheFixedRate()
    {
        IConfiguration configuration = Configuration(new() { ["loop:fixedRate"] = "30" });

        LoopOptions options = Resolve(configuration, static loop => loop.FixedRate = 120);

        await options.FixedRate.Should().BeEqualTo(30);
    }

    [Test]
    public async Task PostConfigureShouldLetConfigurationReplaceTheMaxFrameTime()
    {
        IConfiguration configuration = Configuration(new() { ["loop:maxFrameTime"] = "0.5" });

        LoopOptions options = Resolve(configuration, static loop => loop.MaxFrameTime = 0.1);

        await options.MaxFrameTime.Should().BeEqualTo(0.5);
    }

    [Test]
    public async Task PostConfigureShouldReadTheFixedRateFromTheCommandLine()
    {
        IConfiguration configuration = new ConfigurationBuilder().AddCommandLine(["--loop:fixedRate", "50"]).Build();

        LoopOptions options = Resolve(configuration, static _ => { });

        await options.FixedRate.Should().BeEqualTo(50);
    }

    [Test]
    public async Task ValueShouldThrowWhenTheFixedRateIsZero()
    {
        IConfiguration configuration = Configuration(new() { ["loop:fixedRate"] = "0" });

        KhaosException exception = Assert.Throws<KhaosException>(() => Resolve(configuration, static _ => { }));

        await exception.Message.Should().Contain("loop:fixedRate");
    }

    [Test]
    public async Task ValueShouldThrowWhenTheMaxFrameTimeIsShorterThanAStep()
    {
        KhaosException exception = Assert.Throws<KhaosException>(() => Resolve(Configuration([]), static loop => loop.MaxFrameTime = 0.01));

        await exception.Message.Should().Contain("loop:maxFrameTime");
    }

    [Test]
    public async Task ValueShouldThrowWhenTheMaxFrameTimeIsLongerThanASecond()
    {
        IConfiguration configuration = Configuration(new() { ["loop:maxFrameTime"] = "2" });

        KhaosException exception = Assert.Throws<KhaosException>(() => Resolve(configuration, static _ => { }));

        await exception.Message.Should().Contain("loop:maxFrameTime");
    }

    [Test]
    public async Task ValueShouldPauseWhenMinimizedByDefault()
    {
        LoopOptions options = Resolve(Configuration([]), static _ => { });

        await options.PauseWhenMinimized.Should().BeTrue();
    }

    [Test]
    public async Task PostConfigureShouldLetConfigurationTurnOffThePauseWhenMinimized()
    {
        IConfiguration configuration = Configuration(new() { ["loop:pauseWhenMinimized"] = "false" });

        LoopOptions options = Resolve(configuration, static _ => { });

        await options.PauseWhenMinimized.Should().BeFalse();
    }

    [Test]
    public async Task PostConfigureShouldKeepThePauseTheCodeSetsWhenTheSectionIsEmpty()
    {
        LoopOptions options = Resolve(Configuration([]), static loop => loop.PauseWhenMinimized = false);

        await options.PauseWhenMinimized.Should().BeFalse();
    }

    private static IConfiguration Configuration(Dictionary<string, string?> values)
        => new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    private static LoopOptions Resolve(IConfiguration configuration, Action<LoopOptions> code)
    {
        ServiceCollection services = new();

        services.Configure(code);
        services.BindLoopSettings(configuration);

        return services.BuildServiceProvider().GetRequiredService<IOptions<LoopOptions>>().Value;
    }
}
