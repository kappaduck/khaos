// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;
using KappaDuck.Khaos.Interop.SDL.Primitives;
using KappaDuck.Khaos.Mathematics;

namespace KappaDuck.Khaos.Tests.Events;

internal sealed class WindowResizedTests
{
    [Test]
    public async Task NativeConstructorShouldConvertTheSizeToLogicalUnits()
    {
        SDL_WindowEvent native = new(SDL_EventType.WindowResized, 1280, 720);
        EventContext context = new(2f);

        WindowResized resized = new(in native, in context);

        await resized.Size.Should().BeEqualTo(new SizeI(640, 360));
    }

    [Test]
    public async Task NativeConstructorShouldKeepTheSizeAtAScaleOfOne()
    {
        SDL_WindowEvent native = new(SDL_EventType.WindowResized, 1280, 720);
        EventContext context = new(1f);

        WindowResized resized = new(in native, in context);

        await resized.Size.Should().BeEqualTo(new SizeI(1280, 720));
    }

    [Test]
    public async Task NativeConstructorShouldRoundToTheNearestUnit()
    {
        SDL_WindowEvent native = new(SDL_EventType.WindowResized, 1000, 500);
        EventContext context = new(1.5f);

        WindowResized resized = new(in native, in context);

        await resized.Size.Should().BeEqualTo(new SizeI(667, 333));
    }
}
