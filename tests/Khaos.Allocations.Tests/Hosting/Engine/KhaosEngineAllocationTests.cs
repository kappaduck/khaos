// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;
using KappaDuck.Khaos.Hosting.Engine;
using KappaDuck.Khaos.Input;
using KappaDuck.Khaos.Interop.SDL.Primitives;

namespace KappaDuck.Khaos.Allocations.Tests.Hosting.Engine;

[Category(Categories.Integration)]
[NotInParallel(Sequential.Engine)]
internal sealed class KhaosEngineAllocationTests
{
    private static readonly SDL_Event _keyDown = new(new SDL_KeyboardEvent(SDL_EventType.KeyDown, (int)Scancode.Space, 0));
    private static readonly SDL_Event _resized = new(new SDL_WindowEvent(SDL_EventType.WindowResized, 640, 360));

    private readonly SteppedClock _clock = new();

    private KhaosEngine _engine = null!;

    [Before(Test)]
    public void StartEngine()
    {
        _engine = TestEngine.Start(new SpyGame(), _clock);
    }

    [After(Test)]
    public void StopEngine()
    {
        _engine.Dispose();
    }

    [Test]
    public async Task DispatchShouldNotAllocate()
    {
        Event e = new(new KeyPressed(Scancode.Space));

        long allocated = Allocated.By((_engine, e), static state => state._engine.Dispatch(in state.e));

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task IterateShouldNotAllocate()
    {
        long allocated = Allocated.By(_engine, static engine => engine.Iterate());

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task IterateShouldNotAllocateWithFixedSteps()
    {
        long allocated = Allocated.By((_engine, _clock), static state =>
        {
            state._clock.Advance(4 * SteppedClock.TicksPerStepAt60Hz);
            state._engine.Iterate();
        });

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task ReceiveShouldNotAllocateForAKey()
    {
        long allocated = Allocated.By(_engine, static engine => engine.Receive(in _keyDown));

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task ReceiveShouldNotAllocateForAResize()
    {
        long allocated = Allocated.By(_engine, static engine => engine.Receive(in _resized));

        await allocated.Should().BeEqualTo(0L);
    }
}
