// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting;
using KappaDuck.Khaos.Hosting.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KappaDuck.Khaos.Tests.Hosting.Logging;

[NotInParallel(Sequential.Engine)]
internal sealed class LoggingTests
{
    private const string Physics = "MyGame.Physics";

    private readonly TestHost _host = new();

    [Before(Test)]
    public void ForgetEnvironment() => TestHost.ForgetEnvironment();

    [After(Test)]
    public void Clean() => _host.Delete();

    [Test]
    public async Task ConfigureLoggingShouldApplyToLogging()
    {
        KhaosApplicationBuilder builder = _host.Create().ConfigureLogging(static logging => logging.AddFilter(Physics, LogLevel.Warning));

        ILogger logger = LoggerOf(builder, Physics);

        await logger.IsEnabled(LogLevel.Information).Should().BeFalse();
    }

    [Test]
    public async Task LoggerShouldBeAService()
    {
        KhaosApplicationBuilder builder = _host.Create();

        await using ServiceProvider services = builder.Services.BuildServiceProvider();

        await services.GetService<ILogger<LoggingTests>>().Should().NotBeNull();
    }

    [Test]
    public async Task LoggingSettingsShouldWinOverAFilterInProgram()
    {
        KhaosApplicationBuilder builder = _host.Create($"--logging:logLevel:{Physics}=Warning");
        builder.Logging.AddFilter(Physics, LogLevel.Debug);

        ILogger logger = LoggerOf(builder, Physics);

        await logger.IsEnabled(LogLevel.Information).Should().BeFalse();
    }

    [Test]
    public async Task LoggingShouldFollowTheCommandLine()
    {
        KhaosApplicationBuilder builder = _host.Create("--logging:logLevel:default=Error");

        ILogger logger = LoggerOf(builder, Physics);

        await logger.IsEnabled(LogLevel.Warning).Should().BeFalse();
    }

    [Test]
    public async Task LoggingShouldFollowTheLogLevelSettings()
    {
        _host.Write("appsettings.json", $$"""{ "logging": { "logLevel": { "{{Physics}}": "Warning" } } }""");

        ILogger logger = LoggerOf(_host.Create(), Physics);

        await logger.IsEnabled(LogLevel.Information).Should().BeFalse();
    }

    [Test]
    public async Task LoggingShouldLetTheSettingsOpenNativeLogs()
    {
        KhaosApplicationBuilder builder = _host.Create($"--logging:logLevel:{NativeLog.Category}=Debug");

        ILogger logger = LoggerOf(builder, $"{NativeLog.Category}.Video");

        await logger.IsEnabled(LogLevel.Debug).Should().BeTrue();
    }

    [Test]
    public async Task LoggingShouldStartAtDebugInDevelopment()
    {
        AppContext.SetData(TestHost.BuildEnvironment, Environments.Development);

        ILogger logger = LoggerOf(_host.Create(), Physics);

        await logger.IsEnabled(LogLevel.Debug).Should().BeTrue();
    }

    [Test]
    public async Task LoggingShouldStartAtInformationOutsideDevelopment()
    {
        ILogger logger = LoggerOf(_host.Create(), Physics);

        await logger.IsEnabled(LogLevel.Debug).Should().BeFalse();
    }

    [Test]
    public async Task LoggingShouldStartNativeLogsAtWarning()
    {
        ILogger logger = LoggerOf(_host.Create(), $"{NativeLog.Category}.Video");

        await logger.IsEnabled(LogLevel.Information).Should().BeFalse();
    }

    private static ILogger LoggerOf(KhaosApplicationBuilder builder, string category)
        => builder.Build<ProbedGame>().Services.GetRequiredService<ILoggerFactory>().CreateLogger(category);
}
