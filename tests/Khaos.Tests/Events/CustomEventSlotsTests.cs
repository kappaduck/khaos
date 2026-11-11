// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;
using KappaDuck.Khaos.Interop.SDL.Primitives;

namespace KappaDuck.Khaos.Tests.Events;

internal sealed class CustomEventSlotsTests
{
    private const SDL_EventType Type = SDL_EventType.User;

    [Test]
    public async Task ReleaseAllShouldFreeEverySlot()
    {
        CustomEventSlots<Score> slots = new(Type, capacity: 2);
        _ = slots.TryAdd(new Score(1), out _);
        _ = slots.TryAdd(new Score(2), out _);

        slots.ReleaseAll();

        await slots.TryAdd(new Score(3), out _).Should().BeTrue();
    }

    [Test]
    public async Task ReleaseShouldFreeTheSlot()
    {
        CustomEventSlots<Score> slots = new(Type, capacity: 1);
        _ = slots.TryAdd(new Score(1), out int code);

        slots.Release(code);

        await slots.TryAdd(new Score(2), out _).Should().BeTrue();
    }

    [Test]
    public async Task ReleaseShouldIgnoreTheCodeOfAnEarlierUse()
    {
        CustomEventSlots<Score> slots = new(Type, capacity: 1);
        _ = slots.TryAdd(new Score(1), out int earlier);

        slots.Release(earlier);
        _ = slots.TryAdd(new Score(2), out int current);

        slots.Release(earlier);

        await slots.TryRead(current, out _).Should().BeTrue();
    }

    [Test]
    public async Task TryAddShouldReturnFalseWhenEverySlotIsTaken()
    {
        CustomEventSlots<Score> slots = new(Type, capacity: 1);
        _ = slots.TryAdd(new Score(1), out _);

        bool added = slots.TryAdd(new Score(2), out _);

        await added.Should().BeFalse();
    }

    [Test]
    public async Task TryReadShouldGiveThePayloadItWasGiven()
    {
        CustomEventSlots<Score> slots = new(Type, capacity: 4);
        _ = slots.TryAdd(new Score(42), out int code);

        _ = slots.TryRead(code, out Score payload);

        await payload.Should().BeEqualTo(new Score(42));
    }

    [Test]
    public async Task TryReadShouldReturnFalseForACodeOutsideTheSlots()
    {
        CustomEventSlots<Score> slots = new(Type, capacity: 1);

        bool read = slots.TryRead(5, out _);

        await read.Should().BeFalse();
    }

    [Test]
    public async Task TryReadShouldReturnFalseForTheCodeOfAnEarlierUse()
    {
        CustomEventSlots<Score> slots = new(Type, capacity: 1);
        _ = slots.TryAdd(new Score(1), out int earlier);

        slots.Release(earlier);
        _ = slots.TryAdd(new Score(2), out _);

        bool read = slots.TryRead(earlier, out _);

        await read.Should().BeFalse();
    }

    [Test]
    public async Task TryReadShouldReturnFalseOnceTheSlotIsReleased()
    {
        CustomEventSlots<Score> slots = new(Type, capacity: 1);
        _ = slots.TryAdd(new Score(1), out int code);
        slots.Release(code);

        bool read = slots.TryRead(code, out _);

        await read.Should().BeFalse();
    }

    private readonly record struct Score(int Points);
}
