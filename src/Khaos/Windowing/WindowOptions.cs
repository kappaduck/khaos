// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;

namespace KappaDuck.Khaos.Windowing;

/// <summary>
/// How the game window opens, with its title, its size, and whether it is fullscreen, resizable or borderless.
/// </summary>
/// <remarks>
/// <para>
/// Set them in <c>Program.cs</c> with <c>builder.ConfigureWindow</c>. The <c>window</c> section of your settings,
/// the command line and the environment variables replace what the code sets, except for <see cref="Display"/> and
/// <see cref="Position"/>.
/// </para>
/// <para>
/// Once the game runs, change the window through <see cref="Window"/>.
/// </para>
/// </remarks>
public sealed class WindowOptions
{
    internal const string Section = "window";

    /// <summary>
    /// Gets or sets a value indicating whether the window opens without a title bar or borders.
    /// <see langword="false"/> by default.
    /// </summary>
    public bool Borderless { get; set; }

    /// <summary>
    /// Gets or sets the screen the window opens on. <see langword="null"/> by default, for the primary screen.
    /// </summary>
    /// <remarks>
    /// The settings ignore it, so set it in code. On Linux with Wayland, only a fullscreen window opens on this screen.
    /// </remarks>
    public Display? Display { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the game opens over the whole screen. <see langword="false"/> by default.
    /// </summary>
    public bool Fullscreen { get; set; }

    /// <summary>
    /// Gets or sets the height of the inside of the window, in logical units. 720 by default.
    /// </summary>
    public int Height { get; set; } = 720;

    /// <summary>
    /// Gets or sets where the window opens on <see cref="Display"/>, in logical units from its top-left corner.
    /// <see langword="null"/> by default, to center the window.
    /// </summary>
    /// <remarks>
    /// The settings ignore it, so set it in code. Where <see cref="Window.CanMove"/> is <see langword="false"/>, it is
    /// ignored.
    /// </remarks>
    public PointI? Position { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the player can resize the window. <see langword="false"/> by default.
    /// </summary>
    public bool Resizable { get; set; }

    /// <summary>
    /// Gets or sets the text in the title bar. Empty by default.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the width of the inside of the window, in logical units. 1280 by default.
    /// </summary>
    public int Width { get; set; } = 1280;
}
