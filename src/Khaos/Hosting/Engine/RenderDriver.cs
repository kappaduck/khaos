// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Hosting.Engine;

/// <summary>
/// The graphics API the game draws with.
/// </summary>
public enum RenderDriver
{
    /// <summary>
    /// Khaos picks the best one the computer supports. The default.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// Direct3D 11, on Windows.
    /// </summary>
    Direct3D11 = 1,

    /// <summary>
    /// Direct3D 12, on Windows.
    /// </summary>
    Direct3D12 = 2,

    /// <summary>
    /// OpenGL, on the desktop.
    /// </summary>
    OpenGL = 3,

    /// <summary>
    /// OpenGL ES 2, the one browsers use.
    /// </summary>
    OpenGLES2 = 4,

    /// <summary>
    /// Vulkan, on Windows and Linux.
    /// </summary>
    Vulkan = 5,

    /// <summary>
    /// The modern graphics API of the platform, such as Direct3D 12 or Vulkan.
    /// </summary>
    Gpu = 6,

    /// <summary>
    /// The processor alone, without the graphics card. Slow, but it works everywhere.
    /// </summary>
    Software = 7
}
