// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Allocations.Tests.Events;
using KappaDuck.Khaos.Allocations.Tests.Helpers;
using KappaDuck.Khaos.Events;
using KappaDuck.Khaos.Input;
using KappaDuck.Khaos.Interop.SDL;
using KappaDuck.Khaos.Interop.SDL.Primitives;

namespace KappaDuck.Khaos.Allocations.Tests;

[Category(Categories.Integration)]
[NotInParallel(Sequential.Engine)]
internal sealed class Game2DAllocationTests
{
    private const int FirstMeasuredFrame = 10;
    private const int GiveUpFrame = LastMeasuredFrame + 10;
    private const int LastMeasuredFrame = 110;

    [Test]
    public async Task RunShouldNotAllocateBetweenFrames()
    {
        long[] allocated = new long[2];
        SpyGame game = new() { OnDraw = game => Measure(game, allocated) };

        game.Run();

        await allocated[1].Should().BeEqualTo(allocated[0]);
    }

    [Test]
    public async Task RunShouldNotAllocateBetweenFramesWithEvents()
    {
        long[] allocated = new long[2];

        SpyGame game = new()
        {
            OnDraw = game => PushEvents(game, allocated),
            OnEventReceived = (game, e) => Handle(game, in e, allocated)
        };

        game.Run();

        await allocated[1].Should().BeEqualTo(allocated[0]);
    }

    private static void Handle(SpyGame game, in Event e, long[] allocated)
    {
        switch (e)
        {
            case KeyPressed { Code: Scancode.Escape }:
                game.Exit();
                break;
            case CustomEvent { HarborEvents: ShipDocked { Pier: LastMeasuredFrame } }:
                allocated[1] = GC.GetAllocatedBytesForCurrentThread();
                game.Exit();
                break;
            default:
                break;
        }
    }

    private static void Measure(SpyGame game, long[] allocated)
    {
        if (game.Draws == FirstMeasuredFrame)
            allocated[0] = GC.GetAllocatedBytesForCurrentThread();

        if (game.Draws != LastMeasuredFrame)
            return;

        allocated[1] = GC.GetAllocatedBytesForCurrentThread();
        game.Exit();
    }

    private static void PushEvents(SpyGame game, long[] allocated)
    {
        if (game.Draws == FirstMeasuredFrame)
            allocated[0] = GC.GetAllocatedBytesForCurrentThread();

        if (game.Draws == GiveUpFrame)
            game.Exit();

        _ = EventQueue.TryPush(new ShipDocked(game.Draws));
        PushKey(SDL_EventType.KeyDown);
        PushKey(SDL_EventType.KeyUp);
    }

    private static void PushKey(SDL_EventType type)
    {
        SDL_Event key = new(new SDL_KeyboardEvent(type, (int)Scancode.Space, repeat: 0));
        _ = SDL3.PushEvent(ref key);
    }
}
