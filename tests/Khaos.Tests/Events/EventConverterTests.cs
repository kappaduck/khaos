// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;
using KappaDuck.Khaos.Input;
using KappaDuck.Khaos.Interop.SDL.Primitives;
using KappaDuck.Khaos.Mathematics;

namespace KappaDuck.Khaos.Tests.Events;

internal sealed class EventConverterTests
{
    private const int EscapeScancode = 41;

    private static readonly EventContext _unscaled = new(contentScale: 1f);

    [Test]
    public async Task TryConvertShouldReturnFalseForATypeWithoutACase()
    {
        SDL_Event native = new(SDL_EventType.WindowPixelSizeChanged);

        bool converted = EventConverter.TryConvert(in native, in _unscaled, out _);

        await converted.Should().BeFalse();
    }

    [Test]
    public async Task TryConvertShouldReturnKeyPressedForAKeyDown()
    {
        SDL_Event native = new(new SDL_KeyboardEvent(SDL_EventType.KeyDown, EscapeScancode, repeat: 0));

        _ = EventConverter.TryConvert(in native, in _unscaled, out Event e);

        await (e is KeyPressed { Code: Scancode.Escape }).Should().BeTrue();
    }

    [Test]
    public async Task TryConvertShouldReturnKeyReleasedForAKeyUp()
    {
        SDL_Event native = new(new SDL_KeyboardEvent(SDL_EventType.KeyUp, EscapeScancode, repeat: 0));

        _ = EventConverter.TryConvert(in native, in _unscaled, out Event e);

        await (e is KeyReleased { Code: Scancode.Escape }).Should().BeTrue();
    }

    [Test]
    public async Task TryConvertShouldReturnQuitRequestedForAQuitEvent()
    {
        SDL_Event native = new(SDL_EventType.Quit);

        _ = EventConverter.TryConvert(in native, in _unscaled, out Event e);

        await (e is QuitRequested).Should().BeTrue();
    }

    [Test]
    public async Task TryConvertShouldReturnTrueForATypeWithACase()
    {
        SDL_Event native = new(SDL_EventType.Quit);

        bool converted = EventConverter.TryConvert(in native, in _unscaled, out _);

        await converted.Should().BeTrue();
    }

    [Test]
    public async Task TryConvertShouldReturnWindowFocusGainedForAFocusGainedEvent()
    {
        SDL_Event native = new(SDL_EventType.WindowFocusGained);

        _ = EventConverter.TryConvert(in native, in _unscaled, out Event e);

        await (e is WindowFocusGained).Should().BeTrue();
    }

    [Test]
    public async Task TryConvertShouldReturnWindowFocusLostForAFocusLostEvent()
    {
        SDL_Event native = new(SDL_EventType.WindowFocusLost);

        _ = EventConverter.TryConvert(in native, in _unscaled, out Event e);

        await (e is WindowFocusLost).Should().BeTrue();
    }

    [Test]
    public async Task TryConvertShouldReturnWindowMinimizedForAMinimizedEvent()
    {
        SDL_Event native = new(SDL_EventType.WindowMinimized);

        _ = EventConverter.TryConvert(in native, in _unscaled, out Event e);

        await (e is WindowMinimized).Should().BeTrue();
    }

    [Test]
    public async Task TryConvertShouldReturnWindowResizedInLogicalUnits()
    {
        SDL_Event native = new(new SDL_WindowEvent(SDL_EventType.WindowResized, 1280, 720));
        EventContext context = new(contentScale: 2f);

        _ = EventConverter.TryConvert(in native, in context, out Event e);

        await (e is WindowResized { Size: var size } && size == new SizeI(640, 360)).Should().BeTrue();
    }

    [Test]
    public async Task TryConvertShouldReturnWindowRestoredForARestoredEvent()
    {
        SDL_Event native = new(SDL_EventType.WindowRestored);

        _ = EventConverter.TryConvert(in native, in _unscaled, out Event e);

        await (e is WindowRestored).Should().BeTrue();
    }
}
