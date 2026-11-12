// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Hosting.Logging.Console;

/// <summary>
/// When the level of each log is in color in the terminal.
/// </summary>
public enum ConsoleColors
{
    /// <summary>
    /// In color in a terminal, without color when the output goes to a file or another program. The default.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// Always in color, even when the output goes to a file or another program.
    /// </summary>
    Always = 1,

    /// <summary>
    /// Never in color.
    /// </summary>
    Never = 2
}
