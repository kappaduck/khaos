// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Graphics;

/// <summary>
/// How the game draws on the screen.
/// </summary>
/// <remarks>
/// Set them in <c>Program.cs</c> with <c>builder.ConfigureGraphics</c>. The <c>graphics</c> section of your
/// settings, the command line and the environment variables replace what the code sets.
/// </remarks>
public sealed class GraphicsOptions
{
    internal const string Section = "graphics";

    /// <summary>
    /// Gets or sets the most frames the game draws per second. 0 by default, for no limit.
    /// </summary>
    /// <remarks>
    /// In the settings, the key is <c>graphics:maxFrameRate</c>. A limit saves battery on a laptop.
    /// </remarks>
    public int MaxFrameRate { get; set; }

    /// <summary>
    /// Gets or sets how the game waits for the screen before it shows each frame. <see cref="VSync.Enabled"/> by
    /// default.
    /// </summary>
    /// <remarks>
    /// In the settings, the key is <c>graphics:vsync</c>, and it accepts <c>true</c>, <c>false</c>, <c>"adaptive"</c>
    /// or a number of refreshes per frame. It does nothing in the browser.
    /// </remarks>
    public VSync VSync { get; set; } = VSync.Enabled;
}
