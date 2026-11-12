// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting.Logging.Console;
using Microsoft.Extensions.Logging;

namespace KappaDuck.Khaos.Tests.Hosting.Logging.Console;

internal sealed class KhaosConsoleLoggerProviderTests
{
    private readonly ConsoleRecorder _console = new();
    private readonly FixedTime _time = new(new DateTimeOffset(2026, 11, 11, 19, 32, 7, 481, TimeSpan.Zero), TimeSpan.FromHours(-5));

    [Test]
    public async Task LoggerShouldLeaveTheTimeOutWhenTheFormatIsEmpty()
    {
        ConsoleLogOptions options = new() { TimestampFormat = "" };

        Write(options, LogLevel.Information, "Level Forest loaded.");

        await _console.LastLine.Should().BeEqualTo("[info]:  Level Forest loaded.");
    }

    [Test]
    public async Task LoggerShouldWriteTheLocalTimeFirst()
    {
        ConsoleLogOptions options = new();

        Write(options, LogLevel.Information, "Level Forest loaded.");

        await _console.LastLine.Should().BeEqualTo("14:32:07.481 [info]:  Level Forest loaded.");
    }

    [Test]
    public async Task LoggerShouldWriteTheTimeInUtcWhenAsked()
    {
        ConsoleLogOptions options = new() { UseUtcTimestamp = true };

        Write(options, LogLevel.Information, "Level Forest loaded.");

        await _console.LastLine.Should().BeEqualTo("19:32:07.481 [info]:  Level Forest loaded.");
    }

    [Test]
    public async Task LoggerShouldFollowTheTimestampFormat()
    {
        ConsoleLogOptions options = new() { TimestampFormat = "HH:mm" };

        Write(options, LogLevel.Information, "Level Forest loaded.");

        await _console.LastLine.Should().BeEqualTo("14:32 [info]:  Level Forest loaded.");
    }

    private void Write(ConsoleLogOptions options, LogLevel level, string message)
    {
        _console.TimestampFormat = options.TimestampFormat;
        using KhaosConsoleLoggerProvider provider = new(options, _time, _console);
        ILogger logger = provider.CreateLogger("MyGame.Forest");

        logger.Log(level, default, message, null, static (state, _) => state);
    }

    private sealed class ConsoleRecorder : IConsole
    {
        internal string? LastLine { get; private set; }

        internal string TimestampFormat { get; set; } = string.Empty;

        public void Dispose()
        {
        }

        public void Write(in ConsoleEntry entry) => LastLine = ConsoleLine.Format(in entry, TimestampFormat, false);
    }

    private sealed class FixedTime(DateTimeOffset utcNow, TimeSpan offset) : TimeProvider
    {
        public override TimeZoneInfo LocalTimeZone { get; } = TimeZoneInfo.CreateCustomTimeZone("Khaos test", offset, "Khaos test", "Khaos test");

        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
