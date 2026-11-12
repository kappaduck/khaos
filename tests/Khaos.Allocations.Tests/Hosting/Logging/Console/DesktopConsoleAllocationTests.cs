// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting.Logging.Console;
using Microsoft.Extensions.Logging;

namespace KappaDuck.Khaos.Allocations.Tests.Hosting.Logging.Console;

internal sealed class DesktopConsoleAllocationTests
{
    private static readonly ConsoleEntry Scored = new(DateTimeOffset.UnixEpoch, LogLevel.Debug, "Score is now 10.", null);

    [Test]
    public async Task WriteShouldNotAllocateOnTheGameThread()
    {
        using DesktopConsole console = new(new ConsoleLogOptions(), TextWriter.Null, TextWriter.Null, false, false);

        long allocated = Allocated.By(console, static console => console.Write(in Scored));

        await allocated.Should().BeEqualTo(0L);
    }
}
