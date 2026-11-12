// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting.Engine;

namespace KappaDuck.Khaos.Tests.Hosting.Engine;

internal sealed class DriverNamesTests
{
    [Test]
    public async Task DisplayShouldNameTheWindowsVideoDriver()
    {
        await DriverNames.Display("windows").Should().BeEqualTo("Windows");
    }

    [Test]
    [Arguments("x11", "X11")]
    [Arguments("wayland", "Wayland")]
    [Arguments("emscripten", "Browser")]
    [Arguments("offscreen", "Offscreen")]
    public async Task DisplayShouldNameTheOtherVideoDrivers(string native, string shown)
    {
        await DriverNames.Display(native).Should().BeEqualTo(shown);
    }

    [Test]
    [Arguments("direct3d11", "Direct3D 11")]
    [Arguments("direct3d12", "Direct3D 12")]
    [Arguments("opengl", "OpenGL")]
    [Arguments("opengles2", "OpenGL ES 2")]
    [Arguments("vulkan", "Vulkan")]
    [Arguments("gpu", "GPU")]
    [Arguments("software", "Software")]
    public async Task DisplayShouldNameTheRenderDrivers(string native, string shown)
    {
        await DriverNames.Display(native).Should().BeEqualTo(shown);
    }

    [Test]
    public async Task DisplayShouldKeepAnUnknownName()
    {
        await DriverNames.Display("kmsdrm").Should().BeEqualTo("kmsdrm");
    }

    [Test]
    public async Task DisplayShouldSayUnknownWithoutAName()
    {
        await DriverNames.Display(null).Should().BeEqualTo("unknown");
    }
}
