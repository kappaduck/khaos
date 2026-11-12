// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Hosting.Engine;

internal static class DriverNames
{
    internal static string Display(string? native) => native switch
    {
        null => "unknown",
        SDL3.WindowsVideoDriver => "Windows",
        SDL3.X11VideoDriver => "X11",
        SDL3.WaylandVideoDriver => "Wayland",
        SDL3.EmscriptenVideoDriver => "Browser",
        SDL3.OffscreenVideoDriver => "Offscreen",
        SDL3.Direct3D11Renderer => "Direct3D 11",
        SDL3.Direct3D12Renderer => "Direct3D 12",
        SDL3.OpenGLRenderer => "OpenGL",
        SDL3.OpenGLES2Renderer => "OpenGL ES 2",
        SDL3.VulkanRenderer => "Vulkan",
        SDL3.GpuRenderer => "GPU",
        SDL3.SoftwareRenderer => "Software",
        _ => native
    };
}
