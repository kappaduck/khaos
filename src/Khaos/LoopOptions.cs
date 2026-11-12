// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos;

/// <summary>
/// How the game loop keeps time: the rate of <see cref="Game2D.FixedUpdate"/>, and how much time a slow frame counts.
/// </summary>
/// <remarks>
/// Set them in <c>Program.cs</c> with <c>builder.ConfigureLoop</c>. The <c>loop</c> section of your settings, the
/// command line and the environment variables replace what the code sets.
/// </remarks>
public sealed class LoopOptions
{
    internal const string Section = "loop";

    /// <summary>
    /// Gets or sets how many times per second <see cref="Game2D.FixedUpdate"/> runs. 60 by default.
    /// </summary>
    /// <remarks>
    /// In the settings, the key is <c>loop:fixedRate</c>.
    /// </remarks>
    public int FixedRate { get; set; } = 60;

    /// <summary>
    /// Gets or sets the most time one frame counts, in seconds, so a slow frame does not make the game catch up for
    /// long. 0.25 by default.
    /// </summary>
    /// <remarks>
    /// In the settings, the key is <c>loop:maxFrameTime</c>. It must be at least one fixed step and at most 1 second.
    /// </remarks>
    public double MaxFrameTime { get; set; } = 0.25;

    /// <summary>
    /// Gets or sets whether the game stops while its window is minimized, and picks up where it left off when it comes
    /// back. <see langword="true"/> by default.
    /// </summary>
    /// <remarks>
    /// In the settings, the key is <c>loop:pauseWhenMinimized</c>. To handle it yourself, turn it off and react to
    /// <c>WindowMinimized</c> and <c>WindowRestored</c> in <see cref="Game2D.OnEvent"/>.
    /// </remarks>
    public bool PauseWhenMinimized { get; set; } = true;
}
