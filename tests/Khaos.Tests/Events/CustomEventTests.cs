// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;
using KappaDuck.Khaos.Hosting;
using KappaDuck.Khaos.Interop.SDL.Primitives;
using KappaDuck.Khaos.Tests.Helpers;

namespace KappaDuck.Khaos.Tests.Events;

internal sealed class CustomEventTests
{
    [Test]
    public async Task IsShouldReturnFalseForAnotherPayloadType()
    {
        CustomEvent e = Waiting(new CoinCollected(5));

        bool matched = e.Is<DoorOpened>();

        await matched.Should().BeFalse();
    }

    [Test]
    public async Task IsShouldReturnTrueForThePayloadTypeItCarries()
    {
        CustomEvent e = Waiting(new CoinCollected(5));

        bool matched = e.Is<CoinCollected>();

        await matched.Should().BeTrue();
    }

    [Test]
    public async Task TryGetShouldGiveThePayloadItCarries()
    {
        CustomEvent e = Waiting(new BossDefeated(7));

        _ = e.TryGet(out BossDefeated payload);

        await payload.Should().BeEqualTo(new BossDefeated(7));
    }

    [Test]
    public async Task TryGetShouldReturnFalseForAnotherPayloadType()
    {
        CustomEvent e = Waiting(new BossDefeated(7));

        bool read = e.TryGet(out CoinCollected _);

        await read.Should().BeFalse();
    }

    [Test]
    [Category(Categories.Integration)]
    [NotInParallel(Sequential.Engine)]
    public async Task TryGetShouldReturnFalseForAnEventKeptAfterOnEvent()
    {
        Mock<Game2D> game = Game2D.Mock(1280, 720);

        CustomEvent kept = default;

        game.OnEvent(Any()).Callback(received => kept = received.TryGetValue(out CustomEvent e) ? e : default);

        using KhaosEngine engine = KhaosEngine.Start(game);

        _ = EventQueue.TryPush(new ChestOpened(1));
        _ = SDLEvents.DispatchCustomEvents(engine);

        bool read = kept.TryGet(out ChestOpened _);

        await read.Should().BeFalse();
    }

    [Test]
    public async Task TryGetShouldReturnFalseForTheDefaultEvent()
    {
        CustomEvent e = default;

        bool read = e.TryGet(out CoinCollected _);

        await read.Should().BeFalse();
    }

    private static CustomEvent Waiting<T>(T payload) where T : struct
    {
        CustomEventSlots<T> slots = CustomEventStore<T>.GetOrRegister();
        _ = slots.TryAdd(in payload, out int code);

        SDL_UserEvent native = new(slots.Type, code);
        return new CustomEvent(in native);
    }

    private readonly record struct BossDefeated(int Level);

    private readonly record struct ChestOpened(int Gold);

    private readonly record struct CoinCollected(int Value);

    private readonly record struct DoorOpened(int Room);
}
