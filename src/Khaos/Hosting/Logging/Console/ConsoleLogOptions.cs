// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Hosting.Logging.Console;

/// <summary>
/// How each log looks in the terminal, and in the browser's console.
/// </summary>
/// <remarks>
/// Set them in <c>Program.cs</c> with <c>builder.Logging.ConfigureConsole</c>. The <c>logging:console</c> section of
/// your settings replaces what the code sets.
/// </remarks>
public sealed class ConsoleLogOptions
{
    internal const string Section = "logging:console";

    /// <summary>
    /// Gets or sets when the level of each log is in color. <see cref="ConsoleColors.Auto"/> by default.
    /// </summary>
    public ConsoleColors Colors { get; set; }

    /// <summary>
    /// Gets or sets how many logs can wait to be written to the terminal. 2500 by default.
    /// </summary>
    public int MaxQueueLength { get; set; } = 2500;

    /// <summary>
    /// Gets or sets what a log does when the queue is full. <see cref="ConsoleQueueFullMode.Wait"/> by default.
    /// </summary>
    public ConsoleQueueFullMode QueueFullMode { get; set; }

    /// <summary>
    /// Gets or sets the format of the time at the start of each log, such as <c>HH:mm:ss.fff</c> (the default). An
    /// empty format leaves the time out.
    /// </summary>
    public string TimestampFormat { get; set; } = "HH:mm:ss.fff";

    /// <summary>
    /// Gets or sets whether the time is in UTC rather than the computer's time zone. <see langword="false"/> by
    /// default.
    /// </summary>
    public bool UseUtcTimestamp { get; set; }
}
