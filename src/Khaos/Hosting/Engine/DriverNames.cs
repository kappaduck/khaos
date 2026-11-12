// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Hosting.Engine;

internal static class DriverNames
{
    internal static string Display(string? native) => native switch
    {
        null => "unknown",
        "windows" => "Windows",
        "x11" => "X11",
        "wayland" => "Wayland",
        "emscripten" => "Browser",
        "offscreen" => "Offscreen",
        "direct3d11" => "Direct3D 11",
        "direct3d12" => "Direct3D 12",
        "opengl" => "OpenGL",
        "opengles2" => "OpenGL ES 2",
        "vulkan" => "Vulkan",
        "gpu" => "GPU",
        "software" => "Software",
        _ => native
    };
}
