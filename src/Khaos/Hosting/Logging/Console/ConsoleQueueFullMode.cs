// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Hosting.Logging.Console;

/// <summary>
/// What a log does when the terminal cannot keep up and the queue of logs waiting to be written is full.
/// </summary>
public enum ConsoleQueueFullMode
{
    /// <summary>
    /// The game waits until there is room, so no log is lost. The default.
    /// </summary>
    Wait = 0,

    /// <summary>
    /// The log is dropped, so the game never waits. A line then says how many logs were dropped.
    /// </summary>
    Drop = 1
}
