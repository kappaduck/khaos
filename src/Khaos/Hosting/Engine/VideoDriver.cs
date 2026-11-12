// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Hosting.Engine;

/// <summary>
/// The system the game opens its window with.
/// </summary>
public enum VideoDriver
{
    /// <summary>
    /// Khaos picks the one the platform runs. The default.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// Wayland, on Linux.
    /// </summary>
    Wayland = 1,

    /// <summary>
    /// X11, on Linux, also through XWayland.
    /// </summary>
    X11 = 2,

    /// <summary>
    /// No window on the screen, for tests and servers.
    /// </summary>
    Offscreen = 3
}
