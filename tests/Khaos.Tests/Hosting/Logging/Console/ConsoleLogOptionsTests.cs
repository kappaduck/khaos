// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting;
using KappaDuck.Khaos.Hosting.Logging.Console;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KappaDuck.Khaos.Tests.Hosting.Logging.Console;

[NotInParallel(Sequential.Engine)]
internal sealed class ConsoleLogOptionsTests
{
    private readonly TestHost _host = new();

    [Before(Test)]
    public void ForgetEnvironment() => TestHost.ForgetEnvironment();

    [After(Test)]
    public void Clean() => _host.Delete();

    [Test]
    public async Task ConfigureConsoleShouldApplyToTheConsole()
    {
        KhaosApplicationBuilder builder = _host.Create();
        builder.Logging.ConfigureConsole(static console => console.UseUtcTimestamp = true);

        ConsoleLogOptions console = ConsoleOf(builder);

        await console.UseUtcTimestamp.Should().BeTrue();
    }

    [Test]
    public async Task ConsoleSettingsShouldApplyToTheConsole()
    {
        KhaosApplicationBuilder builder = _host.Create("--logging:console:timestampFormat=HH:mm");

        ConsoleLogOptions console = ConsoleOf(builder);

        await console.TimestampFormat.Should().BeEqualTo("HH:mm");
    }

    [Test]
    public async Task ConsoleSettingsShouldAcceptAnEmptyTimestampFormat()
    {
        KhaosApplicationBuilder builder = _host.Create("--logging:console:timestampFormat=");

        ConsoleLogOptions console = ConsoleOf(builder);

        await console.TimestampFormat.Should().BeEqualTo(string.Empty);
    }

    [Test]
    public async Task ConsoleSettingsShouldWinOverProgram()
    {
        KhaosApplicationBuilder builder = _host.Create("--logging:console:colors=Never");
        builder.Logging.ConfigureConsole(static console => console.Colors = ConsoleColors.Always);

        ConsoleLogOptions console = ConsoleOf(builder);

        await console.Colors.Should().BeEqualTo(ConsoleColors.Never);
    }

    [Test]
    public async Task ConsoleSettingsShouldRefuseUnknownColors()
    {
        KhaosApplicationBuilder builder = _host.Create("--logging:console:colors=Rainbow");

        await Assert.That(() => ConsoleOf(builder)).ThrowsExactly<KhaosException>().WithMessageContaining("logging:console:colors");
    }

    [Test]
    public async Task ConsoleSettingsShouldRefuseAnInvalidTimestampFormat()
    {
        KhaosApplicationBuilder builder = _host.Create("--logging:console:timestampFormat=x");

        await Assert.That(() => ConsoleOf(builder)).ThrowsExactly<KhaosException>().WithMessageContaining("logging:console:timestampFormat");
    }

    [Test]
    public async Task ConsoleSettingsShouldSetTheQueueFullMode()
    {
        KhaosApplicationBuilder builder = _host.Create("--logging:console:queueFullMode=Drop");

        ConsoleLogOptions console = ConsoleOf(builder);

        await console.QueueFullMode.Should().BeEqualTo(ConsoleQueueFullMode.Drop);
    }

    [Test]
    public async Task ConsoleSettingsShouldSetTheMaxQueueLength()
    {
        KhaosApplicationBuilder builder = _host.Create("--logging:console:maxQueueLength=100");

        ConsoleLogOptions console = ConsoleOf(builder);

        await console.MaxQueueLength.Should().BeEqualTo(100);
    }

    [Test]
    public async Task ConsoleSettingsShouldRefuseAnUnknownQueueFullMode()
    {
        KhaosApplicationBuilder builder = _host.Create("--logging:console:queueFullMode=Block");

        await Assert.That(() => ConsoleOf(builder)).ThrowsExactly<KhaosException>().WithMessageContaining("logging:console:queueFullMode");
    }

    [Test]
    public async Task ConsoleSettingsShouldRefuseAQueueWithoutRoom()
    {
        KhaosApplicationBuilder builder = _host.Create("--logging:console:maxQueueLength=0");

        await Assert.That(() => ConsoleOf(builder)).ThrowsExactly<KhaosException>().WithMessageContaining("logging:console:maxQueueLength");
    }

    [Test]
    public async Task ConfigureConsoleShouldRefuseAQueueWithoutRoom()
    {
        KhaosApplicationBuilder builder = _host.Create();
        builder.Logging.ConfigureConsole(static console => console.MaxQueueLength = 0);

        await Assert.That(() => ConsoleOf(builder)).ThrowsExactly<KhaosException>().WithMessageContaining("logging:console:maxQueueLength");
    }

    private static ConsoleLogOptions ConsoleOf(KhaosApplicationBuilder builder)
        => builder.Build<ProbedGame>().Services.GetRequiredService<IOptions<ConsoleLogOptions>>().Value;
}
