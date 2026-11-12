// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting.Logging.Console;
using Microsoft.Extensions.Logging;

namespace KappaDuck.Khaos.Tests.Hosting.Logging.Console;

internal sealed class ConsoleLineTests
{
    private const string TimeFormat = "HH:mm:ss.fff";

    private static readonly DateTimeOffset Afternoon = new(2026, 11, 11, 14, 32, 7, 481, TimeSpan.Zero);

    [Test]
    public async Task FormatShouldWriteTheExceptionOnTheNextLines()
    {
        InvalidOperationException exception = new("No ball left.");

        ConsoleEntry entry = new(null, LogLevel.Error, "The round could not start.", exception);
        string line = ConsoleLine.Format(in entry, TimeFormat, false);

        await line.Should().BeEqualTo($"[fail]:  The round could not start.{Environment.NewLine}{exception}");
    }

    [Test]
    public async Task FormatShouldWriteTheTagThenTheMessage()
    {
        ConsoleEntry entry = new(null, LogLevel.Information, "The ball fell off the table.", null);
        string line = ConsoleLine.Format(in entry, TimeFormat, false);

        await line.Should().BeEqualTo("[info]:  The ball fell off the table.");
    }

    [Test]
    [Arguments(LogLevel.Trace, "trace")]
    [Arguments(LogLevel.Debug, "debug")]
    [Arguments(LogLevel.Information, "info")]
    [Arguments(LogLevel.Warning, "warn")]
    [Arguments(LogLevel.Error, "fail")]
    [Arguments(LogLevel.Critical, "crit")]
    public async Task TagShouldNameTheLevelInShort(LogLevel level, string tag)
    {
        await ConsoleLine.Tag(level).Should().BeEqualTo(tag);
    }

    [Test]
    public async Task FormatShouldStartTheMessageInTheSameColumnAfterALongerTag()
    {
        ConsoleEntry entry = new(null, LogLevel.Debug, "Score is now 10.", null);
        string line = ConsoleLine.Format(in entry, TimeFormat, false);

        await line.Should().BeEqualTo("[debug]: Score is now 10.");
    }

    [Test]
    public async Task FormatShouldPutTheTimeBeforeTheTag()
    {
        ConsoleEntry entry = new(Afternoon, LogLevel.Information, "Level Forest loaded.", null);
        string line = ConsoleLine.Format(in entry, TimeFormat, false);

        await line.Should().BeEqualTo("14:32:07.481 [info]:  Level Forest loaded.");
    }

    [Test]
    public async Task FormatShouldColorTheTagWhenAsked()
    {
        ConsoleEntry entry = new(Afternoon, LogLevel.Information, "Level Forest loaded.", null);
        string line = ConsoleLine.Format(in entry, TimeFormat, true);

        await line.Should().BeEqualTo("14:32:07.481 \e[32m[info]\e[39m:  Level Forest loaded.");
    }

    [Test]
    [Arguments(LogLevel.Trace, "\e[37m")]
    [Arguments(LogLevel.Debug, "\e[37m")]
    [Arguments(LogLevel.Information, "\e[32m")]
    [Arguments(LogLevel.Warning, "\e[93m")]
    [Arguments(LogLevel.Error, "\e[91m")]
    [Arguments(LogLevel.Critical, "\e[91m")]
    public async Task FormatShouldColorEachLevel(LogLevel level, string color)
    {
        ConsoleEntry entry = new(null, level, "Level Forest loaded.", null);
        string line = ConsoleLine.Format(in entry, TimeFormat, true);

        await line.Should().StartWith(color);
    }
}
