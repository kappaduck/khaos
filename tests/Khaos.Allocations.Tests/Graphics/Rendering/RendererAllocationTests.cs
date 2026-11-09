// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Allocations.Tests.Helpers;
using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Graphics.Rendering;
using KappaDuck.Khaos.Hosting;
using KappaDuck.Khaos.Windowing;
using Microsoft.Extensions.Logging.Abstractions;

namespace KappaDuck.Khaos.Allocations.Tests.Graphics.Rendering;

[Category(Categories.Integration)]
[NotInParallel(Sequential.Engine)]
internal sealed class RendererAllocationTests
{
    private readonly WindowOptions _options = new() { Title = "Duck Quest", Width = 640, Height = 360 };

    private NativeSession _session = null!;
    private Window _window = null!;
    private Renderer _renderer = null!;

    [Before(Test)]
    public void StartEngine()
    {
        _session = NativeSession.Start(new AppMetadata() { Name = "Duck Quest" });
        _window = Window.Open(_options);
        _renderer = Renderer.Create(_window, VSync.Disabled, NullLogger<Renderer>.Instance);
    }

    [After(Test)]
    public void StopEngine()
    {
        _renderer.Destroy();
        _window.Close();
        _session.Dispose();
    }

    [Test]
    public async Task ClearAndPresentShouldNotAllocate()
    {
        long allocated = Allocated.By(_renderer, static renderer =>
        {
            renderer.Clear(Color.Teal);
            renderer.Present();
        });

        await allocated.Should().BeEqualTo(0L);
    }
}
