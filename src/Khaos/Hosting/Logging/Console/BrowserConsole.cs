// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.Browser;
using System.Runtime.Versioning;

namespace KappaDuck.Khaos.Hosting.Logging.Console;

[SupportedOSPlatform("browser")]
internal sealed class BrowserConsole(string timestampFormat) : IConsole
{
    public void Dispose()
    {
    }

    public void Write(in ConsoleEntry entry) => KhaosPage.Log((int)entry.Level, ConsoleLine.Format(in entry, timestampFormat, false));
}
