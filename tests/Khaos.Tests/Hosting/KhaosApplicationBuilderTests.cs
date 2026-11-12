// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KappaDuck.Khaos.Tests.Hosting;

[NotInParallel(Sequential.Engine)]
internal sealed class KhaosApplicationBuilderTests
{
    private const string BuildEnvironment = TestHost.BuildEnvironment;
    private const string DotnetEnvironment = TestHost.DotnetEnvironment;
    private const string DotnetLives = "DOTNET_GAME__LIVES";
    private const string KhaosEnvironment = TestHost.KhaosEnvironment;
    private const string KhaosLives = "KHAOS_GAME__LIVES";
    private const string Lives = "game:lives";

    private readonly TestHost _host = new();

    [Before(Test)]
    public void ForgetEnvironment()
    {
        TestHost.ForgetEnvironment();
        ForgetLives();
    }

    [After(Test)]
    public void Clean()
    {
        ForgetLives();
        _host.Delete();
    }

    [Test]
    public async Task ConfigurationShouldIgnoreDotnetVariables()
    {
        Environment.SetEnvironmentVariable(DotnetLives, "7");

        KhaosApplicationBuilder builder = _host.Create();

        await builder.Configuration[Lives].Should().BeNull();
    }

    [Test]
    public async Task ConfigurationShouldIgnoreTheFileOfAnotherEnvironment()
    {
        WriteSettings("appsettings.Staging.json", 5);

        KhaosApplicationBuilder builder = _host.Create();

        await builder.Configuration[Lives].Should().BeNull();
    }

    [Test]
    public async Task ConfigurationShouldPreferKhaosVariablesOverTheEnvironmentFile()
    {
        AppContext.SetData(BuildEnvironment, Environments.Development);
        WriteSettings("appsettings.Development.json", 5);
        Environment.SetEnvironmentVariable(KhaosLives, "7");

        KhaosApplicationBuilder builder = _host.Create();

        await builder.Configuration[Lives].Should().BeEqualTo("7");
    }

    [Test]
    public async Task ConfigurationShouldPreferTheCommandLineOverKhaosVariables()
    {
        Environment.SetEnvironmentVariable(KhaosLives, "7");

        KhaosApplicationBuilder builder = _host.Create("--game:lives", "9");

        await builder.Configuration[Lives].Should().BeEqualTo("9");
    }

    [Test]
    public async Task ConfigurationShouldPreferTheEnvironmentFileOverAppSettings()
    {
        AppContext.SetData(BuildEnvironment, Environments.Development);
        WriteSettings("appsettings.json", 3);
        WriteSettings("appsettings.Development.json", 5);

        KhaosApplicationBuilder builder = _host.Create();

        await builder.Configuration[Lives].Should().BeEqualTo("5");
    }

    [Test]
    public async Task ConfigurationShouldReadAppSettings()
    {
        WriteSettings("appsettings.json", 3);

        KhaosApplicationBuilder builder = _host.Create();

        await builder.Configuration[Lives].Should().BeEqualTo("3");
    }

    [Test]
    public async Task ConfigureEngineShouldReturnTheBuilder()
    {
        KhaosApplicationBuilder builder = _host.Create();

        await builder.ConfigureEngine(static _ => { }).Should().BeSameReferenceAs(builder);
    }

    [Test]
    public async Task ConfigureLoggingShouldReturnTheBuilder()
    {
        KhaosApplicationBuilder builder = _host.Create();

        await builder.ConfigureLogging(static _ => { }).Should().BeSameReferenceAs(builder);
    }

    [Test]
    public async Task ContentRootPathShouldBeTheFolderOfTheGame()
    {
        KhaosApplicationBuilder builder = KhaosApplication.CreateBuilder();

        await builder.Environment.ContentRootPath.Should().BeEqualTo(AppContext.BaseDirectory);
    }

    [Test]
    public async Task EnvironmentShouldBeAService()
    {
        KhaosApplicationBuilder builder = _host.Create();

        await using ServiceProvider services = builder.Services.BuildServiceProvider();

        await services.GetRequiredService<IHostEnvironment>().Should().BeSameReferenceAs(builder.Environment);
    }

    [Test]
    public async Task EnvironmentShouldBeProductionWhenNothingNamesIt()
    {
        KhaosApplicationBuilder builder = _host.Create();

        await builder.Environment.EnvironmentName.Should().BeEqualTo(Environments.Production);
    }

    [Test]
    public async Task EnvironmentShouldComeFromTheBuild()
    {
        AppContext.SetData(BuildEnvironment, Environments.Staging);

        KhaosApplicationBuilder builder = _host.Create();

        await builder.Environment.EnvironmentName.Should().BeEqualTo(Environments.Staging);
    }

    [Test]
    public async Task EnvironmentShouldPreferDotnetEnvironmentOverTheBuild()
    {
        AppContext.SetData(BuildEnvironment, Environments.Staging);
        Environment.SetEnvironmentVariable(DotnetEnvironment, Environments.Development);

        KhaosApplicationBuilder builder = _host.Create();

        await builder.Environment.EnvironmentName.Should().BeEqualTo(Environments.Development);
    }

    [Test]
    public async Task EnvironmentShouldPreferKhaosEnvironmentOverDotnetEnvironment()
    {
        Environment.SetEnvironmentVariable(DotnetEnvironment, Environments.Development);
        Environment.SetEnvironmentVariable(KhaosEnvironment, Environments.Staging);

        KhaosApplicationBuilder builder = _host.Create();

        await builder.Environment.EnvironmentName.Should().BeEqualTo(Environments.Staging);
    }

    [Test]
    public async Task EnvironmentShouldPreferTheCommandLineOverKhaosEnvironment()
    {
        Environment.SetEnvironmentVariable(KhaosEnvironment, Environments.Staging);

        KhaosApplicationBuilder builder = _host.Create("--environment", "QA");

        await builder.Environment.EnvironmentName.Should().BeEqualTo("QA");
    }

    [Test]
    public async Task EnvironmentShouldRefuseANameOfSpaces()
    {
        await Assert.That(() => _host.Create("--environment", "  ")).ThrowsExactly<KhaosException>().WithMessage(GameEnvironment.EmptyName);
    }

    [Test]
    public async Task EnvironmentShouldRefuseAnEmptyName()
    {
        await Assert.That(() => _host.Create("--environment=")).ThrowsExactly<KhaosException>().WithMessage(GameEnvironment.EmptyName);
    }

    private static void ForgetLives()
    {
        Environment.SetEnvironmentVariable(DotnetLives, null);
        Environment.SetEnvironmentVariable(KhaosLives, null);
    }

    private void WriteSettings(string name, int lives) => _host.Write(name, $$"""{ "game": { "lives": {{lives}} } }""");
}
