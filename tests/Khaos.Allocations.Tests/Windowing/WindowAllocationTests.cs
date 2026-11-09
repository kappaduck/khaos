// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Allocations.Tests.Helpers;
using KappaDuck.Khaos.Hosting;
using KappaDuck.Khaos.Windowing;

namespace KappaDuck.Khaos.Allocations.Tests.Windowing;

[Category(Categories.Integration)]
[NotInParallel(Sequential.Engine)]
internal sealed class WindowAllocationTests
{
    private readonly WindowOptions _options = new() { Title = "Duck Quest", Width = 640, Height = 360 };

    private NativeSession _session = null!;
    private Window _window = null!;

    [Before(Test)]
    public void StartEngine()
    {
        _session = NativeSession.Start(new AppMetadata() { Name = "Duck Quest" });
        _window = Window.Open(_options);
    }

    [After(Test)]
    public void StopEngine()
    {
        _window.Close();
        _session.Dispose();
    }

    [Test]
    public async Task SetTitleShouldNotAllocateFromASpan()
    {
        long allocated = Allocated.By(_window, static window => window.SetTitle("Duck Quest - Level 2".AsSpan()));

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task SetTitleShouldNotAllocateFromAnInterpolatedString()
    {
        const int frame = 1234;

        long allocated = Allocated.By((window: _window, frame), static state => state.window.SetTitle($"Frame {state.frame}"));

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task SizeShouldNotAllocate()
    {
        long allocated = Allocated.By(_window, static window => _ = window.Size);

        await allocated.Should().BeEqualTo(0L);
    }
}
