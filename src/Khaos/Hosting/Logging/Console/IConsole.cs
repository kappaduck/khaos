// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Hosting.Logging.Console;

internal interface IConsole : IDisposable
{
    void Write(in ConsoleEntry entry);
}
