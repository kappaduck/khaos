// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;
using KappaDuck.Khaos.Hosting.Engine;
using KappaDuck.Khaos.Interop.SDL.Primitives;

namespace KappaDuck.Khaos.Allocations.Tests.Events;

[Category(Categories.Integration)]
[NotInParallel(Sequential.Engine)]
internal sealed class CustomEventAllocationTests
{
    private KhaosEngine _engine = null!;

    [Before(Test)]
    public void StartEngine()
    {
        _engine = TestEngine.Start(new SpyGame());
    }

    [After(Test)]
    public void StopEngine()
    {
        _engine.Dispose();
    }

    [Test]
    public async Task PropertyPatternThroughAGroupShouldNotAllocate()
    {
        SDL_Event native = Waiting(new CargoLoaded(12));
        Event e = new(new CustomEvent(in native.User));

        long allocated = Allocated.By(e, static e => e is CustomEvent { HarborEvents: CargoLoaded { Crates: > 10 } });

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task PropertyPatternThroughAGroupShouldNotAllocateForAnEventThatHoldsAString()
    {
        SDL_Event native = Waiting(new CaptainNamed("Morgane"));
        Event e = new(new CustomEvent(in native.User));

        long allocated = Allocated.By(e, static e => e is CustomEvent { HarborEvents: CaptainNamed { Name.Length: > 0 } });

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task ReceiveShouldNotAllocateForACustomEvent()
    {
        SDL_Event native = Waiting(new GemFound(3));

        long allocated = Allocated.By((_engine, native), static state => state._engine.Receive(in state.native));

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task TryGetShouldNotAllocate()
    {
        SDL_Event native = Waiting(new KeyFound(1));
        CustomEvent e = new(in native.User);

        long allocated = Allocated.By(e, static e => e.TryGet(out KeyFound _));

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task TryPushShouldNotAllocate()
    {
        long allocated = Allocated.By(new MapFound(2), static payload => EventQueue.TryPush(in payload));

        await allocated.Should().BeEqualTo(0L);
    }

    private static SDL_Event Waiting<T>(T payload) where T : struct
    {
        CustomEventSlots<T> slots = CustomEventStore<T>.GetOrRegister();
        _ = slots.TryAdd(in payload, out int code);

        return new SDL_Event(new SDL_UserEvent(slots.Type, code));
    }

    private readonly record struct GemFound(int Count);

    private readonly record struct KeyFound(int Door);

    private readonly record struct MapFound(int Level);
}
