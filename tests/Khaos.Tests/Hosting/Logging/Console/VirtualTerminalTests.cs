// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting.Logging.Console;
using TUnit.Core.Enums;

namespace KappaDuck.Khaos.Tests.Hosting.Logging.Console;

internal sealed class VirtualTerminalTests
{
    [Test]
    [RunOn(OS.Linux)]
    public async Task EnableShouldTurnColorsOnOutsideWindows()
    {
        using VirtualTerminal terminal = VirtualTerminal.Enable();

        await terminal.OnOutput.Should().BeTrue();
    }

    [Test]
    [RunOn(OS.Windows)]
    public async Task EnableShouldTurnColorsOnOnlyWhenTheOutputIsATerminal()
    {
        using VirtualTerminal terminal = VirtualTerminal.Enable();

        await terminal.OnOutput.Should().BeEqualTo(!System.Console.IsOutputRedirected);
    }

    [Test]
    [RunOn(OS.Windows)]
    public async Task EnableShouldTurnColorsOnOnlyWhenTheErrorOutputIsATerminal()
    {
        using VirtualTerminal terminal = VirtualTerminal.Enable();

        await terminal.OnError.Should().BeEqualTo(!System.Console.IsErrorRedirected);
    }
}
