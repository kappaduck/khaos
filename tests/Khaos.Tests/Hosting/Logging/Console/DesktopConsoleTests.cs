// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting.Logging.Console;
using Microsoft.Extensions.Logging;

namespace KappaDuck.Khaos.Tests.Hosting.Logging.Console;

internal sealed class DesktopConsoleTests
{
    private readonly StringWriter _error = new();
    private readonly ConsoleLogOptions _options = new() { TimestampFormat = "" };
    private readonly StringWriter _output = new();

    [Test]
    public async Task WriteShouldSendInformationToTheOutput()
    {
        ConsoleEntry entry = new(null, LogLevel.Information, "Level Forest loaded.", null);

        Write(in entry);

        await _output.ToString().Should().BeEqualTo($"[info]:  Level Forest loaded.{Environment.NewLine}");
    }

    [Test]
    public async Task WriteShouldSendAWarningToTheErrorOutput()
    {
        ConsoleEntry entry = new(null, LogLevel.Warning, "The ball fell off the table.", null);

        Write(in entry);

        await _error.ToString().Should().BeEqualTo($"[warn]:  The ball fell off the table.{Environment.NewLine}");
    }

    [Test]
    public async Task WriteShouldColorTheTagWhenTheOutputHasColors()
    {
        ConsoleEntry entry = new(null, LogLevel.Information, "Level Forest loaded.", null);

        Write(in entry, true);

        await _output.ToString().Should().BeEqualTo($"\e[32m[info]\e[39m:  Level Forest loaded.{Environment.NewLine}");
    }

    private void Write(in ConsoleEntry entry, bool colored = false)
    {
        using DesktopConsole console = new(_options, _output, _error, colored, colored);

        console.Write(in entry);
    }
}
