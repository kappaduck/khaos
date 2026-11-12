// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;
using KappaDuck.Khaos.Input;
using KappaDuck.Khaos.Interop.SDL.Primitives;
using KappaDuck.Khaos.Mathematics;

namespace KappaDuck.Khaos.Allocations.Tests.Events;

internal sealed class EventAllocationTests
{
    private const int EscapeScancode = 41;

    private static readonly SDL_Event _keyDown = new(new SDL_KeyboardEvent(SDL_EventType.KeyDown, EscapeScancode, 0));
    private static readonly SDL_Event _quit = new(SDL_EventType.Quit);
    private static readonly SDL_Event _resized = new(new SDL_WindowEvent(SDL_EventType.WindowResized, 1280, 720));
    private static readonly EventContext _scaled = new(2f);

    [Test]
    public async Task PropertyPatternShouldNotAllocate()
    {
        Event e = new(new KeyPressed(Scancode.Escape));

        long allocated = Allocated.By(e, static e => e is KeyPressed { Code: Scancode.Escape });

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task SwitchShouldNotAllocate()
    {
        Event e = new(new WindowResized(new SizeI(640, 360)));

        long allocated = Allocated.By(e, static e => Match(in e));

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task TryConvertShouldNotAllocateForAKeyDown()
    {
        long allocated = Allocated.By(_keyDown, static native => EventConverter.TryConvert(in native, in _scaled, out _));

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task TryConvertShouldNotAllocateForAQuit()
    {
        long allocated = Allocated.By(_quit, static native => EventConverter.TryConvert(in native, in _scaled, out _));

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task TryConvertShouldNotAllocateForAResize()
    {
        long allocated = Allocated.By(_resized, static native => EventConverter.TryConvert(in native, in _scaled, out _));

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task TypePatternShouldNotAllocate()
    {
        Event e = new(new WindowFocusLost());

        long allocated = Allocated.By(e, static e => e is WindowFocusLost);

        await allocated.Should().BeEqualTo(0L);
    }

    private static int Match(in Event e) => e switch
    {
        QuitRequested => 1,
        KeyPressed { Code: Scancode.Space } => 2,
        WindowResized { Size.Width: > 100 } => 3,
        _ => 0
    };
}
