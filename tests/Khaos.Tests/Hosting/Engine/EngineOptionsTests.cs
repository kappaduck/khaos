// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting;
using KappaDuck.Khaos.Hosting.Engine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KappaDuck.Khaos.Tests.Hosting.Engine;

[NotInParallel(Sequential.Engine)]
internal sealed class EngineOptionsTests
{
    private readonly TestHost _host = new();

    [Before(Test)]
    public void ForgetEnvironment() => TestHost.ForgetEnvironment();

    [After(Test)]
    public void Clean() => _host.Delete();

    [Test]
    public async Task ConfigureEngineShouldReachTheEngineOptions()
    {
        KhaosApplicationBuilder builder = _host.Create().ConfigureEngine(static engine => engine.Native[NativeNames.AllowScreenSaver] = "1");

        EngineOptions engine = EngineOf(builder);

        await engine.Native[NativeNames.AllowScreenSaver].Should().BeEqualTo("1");
    }

    [Test]
    public async Task EngineSettingsShouldReadTheRenderDriver()
    {
        EngineOptions engine = EngineOf(_host.Create("--engine:renderDriver=VULKAN"));

        await engine.RenderDriver.Should().BeEqualTo(RenderDriver.Vulkan);
    }

    [Test]
    public async Task EngineSettingsShouldReadTheVideoDriver()
    {
        EngineOptions engine = EngineOf(_host.Create("--engine:videoDriver=x11"));

        await engine.VideoDriver.Should().BeEqualTo(VideoDriver.X11);
    }

    [Test]
    public async Task EngineSettingsShouldRefuseAnUnknownRenderDriver()
    {
        KhaosApplicationBuilder builder = _host.Create("--engine:renderDriver=glide");

        await Assert.That(() => EngineOf(builder)).ThrowsExactly<KhaosException>().WithMessageContaining("engine:renderDriver");
    }

    [Test]
    public async Task EngineSettingsShouldRefuseAnUnknownVideoDriver()
    {
        KhaosApplicationBuilder builder = _host.Create("--engine:videoDriver=7");

        await Assert.That(() => EngineOf(builder)).ThrowsExactly<KhaosException>().WithMessageContaining("engine:videoDriver");
    }

    [Test]
    public async Task EngineSettingsShouldFillNative()
    {
        _host.Write("appsettings.json", $$"""{ "engine": { "native": { "{{NativeNames.AllowScreenSaver}}": "0" } } }""");

        EngineOptions engine = EngineOf(_host.Create());

        await engine.Native[NativeNames.AllowScreenSaver].Should().BeEqualTo("0");
    }

    [Test]
    public async Task EngineSettingsShouldPreferTheCommandLineOverAppSettings()
    {
        _host.Write("appsettings.json", $$"""{ "engine": { "native": { "{{NativeNames.AllowScreenSaver}}": "0" } } }""");

        EngineOptions engine = EngineOf(_host.Create($"--engine:native:{NativeNames.AllowScreenSaver}=1"));

        await engine.Native[NativeNames.AllowScreenSaver].Should().BeEqualTo("1");
    }

    [Test]
    public async Task EngineSettingsShouldWinOverProgram()
    {
        KhaosApplicationBuilder builder = _host.Create($"--engine:native:{NativeNames.AllowScreenSaver}=0")
            .ConfigureEngine(static engine => engine.Native[NativeNames.AllowScreenSaver] = "1");

        EngineOptions engine = EngineOf(builder);

        await engine.Native[NativeNames.AllowScreenSaver].Should().BeEqualTo("0");
    }

    [Test]
    public async Task NativeShouldIgnoreTheCaseOfNames()
    {
        KhaosApplicationBuilder builder = _host.Create("--engine:native:sdl_video_allow_screensaver=0")
            .ConfigureEngine(static engine => engine.Native[NativeNames.AllowScreenSaver] = "1");

        EngineOptions engine = EngineOf(builder);

        await engine.Native.Count.Should().BeEqualTo(1);
    }

    [Test]
    public async Task NativeShouldRefuseAnEngineInvariant()
    {
        KhaosApplicationBuilder builder = _host.Create($"--engine:native:{NativeNames.NoSignalHandlers}=0");

        await Assert.That(() => EngineOf(builder)).ThrowsExactly<KhaosException>().WithMessageContaining(NativeNames.NoSignalHandlers);
    }

    [Test]
    public async Task NativeShouldRefuseANameOwnedByAnOption()
    {
        KhaosApplicationBuilder builder = _host.Create($"--engine:native:{NativeNames.MainCallbackRate}=30");

        await Assert.That(() => EngineOf(builder)).ThrowsExactly<KhaosException>().WithMessageContaining("graphics:maxFrameRate");
    }

    [Test]
    public async Task NativeShouldRefuseANameWithoutPrefix()
    {
        KhaosApplicationBuilder builder = _host.Create("--engine:native:VIDEO_ALLOW_SCREENSAVER=1");

        await Assert.That(() => EngineOf(builder)).ThrowsExactly<KhaosException>().WithMessageContaining("VIDEO_ALLOW_SCREENSAVER");
    }

    [Test]
    public async Task NativeShouldRefuseTheRenderDriver()
    {
        KhaosApplicationBuilder builder = _host.Create($"--engine:native:{NativeNames.RenderDriver}=software");

        await Assert.That(() => EngineOf(builder)).ThrowsExactly<KhaosException>().WithMessageContaining("engine:renderDriver");
    }

    [Test]
    public async Task NativeShouldRefuseTheVideoDriver()
    {
        KhaosApplicationBuilder builder = _host.Create($"--engine:native:{NativeNames.VideoDriver}=x11");

        await Assert.That(() => EngineOf(builder)).ThrowsExactly<KhaosException>().WithMessageContaining("engine:videoDriver");
    }

    [Test]
    public async Task RenderDriverShouldBeAutoByDefault()
    {
        EngineOptions engine = EngineOf(_host.Create());

        await engine.RenderDriver.Should().BeEqualTo(RenderDriver.Auto);
    }

    [Test]
    public async Task VideoDriverShouldBeAutoByDefault()
    {
        EngineOptions engine = EngineOf(_host.Create());

        await engine.VideoDriver.Should().BeEqualTo(VideoDriver.Auto);
    }

    private static EngineOptions EngineOf(KhaosApplicationBuilder builder)
        => builder.Build<ProbedGame>().Services.GetRequiredService<IOptions<EngineOptions>>().Value;
}
