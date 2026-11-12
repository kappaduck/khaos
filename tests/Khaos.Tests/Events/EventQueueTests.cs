// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;
using KappaDuck.Khaos.Hosting.Engine;

namespace KappaDuck.Khaos.Tests.Events;

[Category(Categories.Integration)]
[NotInParallel(Sequential.Engine)]
internal sealed class EventQueueTests
{
    [Test]
    public async Task RegisterShouldAcceptTheSameCapacityTwice()
    {
        EventQueue.Register<SectorTimed>(4);

        await Assert.That(static () => EventQueue.Register<SectorTimed>(4)).ThrowsNothing();
    }

    [Test]
    public async Task RegisterShouldRefuseACapacityAboveTheLimit()
    {
        await Assert.That(static () => EventQueue.Register<RaceStarted>(65537)).ThrowsExactly<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task RegisterShouldRefuseACapacityOfZero()
    {
        await Assert.That(static () => EventQueue.Register<RaceStarted>(0)).ThrowsExactly<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task RegisterShouldRefuseAnotherCapacityOnceTheTypeIsRegistered()
    {
        EventQueue.Register<RaceFinished>(4);

        await Assert.That(static () => EventQueue.Register<RaceFinished>(8)).ThrowsExactly<InvalidOperationException>();
    }

    [Test]
    public async Task TryPushShouldFindAFreeSlotAfterTheEngineStopsWithEventsWaiting()
    {
        EventQueue.Register<CheckpointReached>(1);
        KhaosEngine first = TestEngine.Start(Game2D.Mock(1280, 720));
        _ = EventQueue.TryPush(new CheckpointReached(1));
        first.Dispose();
        using KhaosEngine engine = TestEngine.Start(Game2D.Mock(1280, 720));

        bool pushed = EventQueue.TryPush(new CheckpointReached(2));

        await pushed.Should().BeTrue();
    }

    [Test]
    public async Task TryPushShouldFreeTheSlotOnceTheEventIsDispatched()
    {
        EventQueue.Register<BoostUsed>(1);
        using KhaosEngine engine = TestEngine.Start(Game2D.Mock(1280, 720));
        _ = EventQueue.TryPush(new BoostUsed(1));
        _ = SDLEvents.DispatchCustomEvents(engine);

        bool pushed = EventQueue.TryPush(new BoostUsed(2));

        await pushed.Should().BeTrue();
    }

    [Test]
    public void TryPushShouldReachOnEventAsACustomEvent()
    {
        Mock<Game2D> game = Game2D.Mock(1280, 720);
        game.Draw(Any(), Any()).Callback(static () => _ = EventQueue.TryPush(new LapCompleted(3)));
        game.OnEvent(static received => IsLapThree(received)).Callback(game.Object.Exit);

        TestEngine.Run(game);

        game.Draw(Any(), Any()).WasCalled(Times.Once);
    }

    [Test]
    public async Task TryPushShouldReturnFalseWhenEverySlotIsWaiting()
    {
        EventQueue.Register<TireChanged>(1);
        using KhaosEngine engine = TestEngine.Start(Game2D.Mock(1280, 720));
        _ = EventQueue.TryPush(new TireChanged(1));

        bool pushed = EventQueue.TryPush(new TireChanged(2));

        await pushed.Should().BeFalse();
    }

    [Test]
    public async Task TryPushShouldReturnFalseWithoutAnEngine()
    {
        bool pushed = EventQueue.TryPush(new PitStop(1));

        await pushed.Should().BeFalse();
    }

    [Test]
    public async Task TryPushShouldReturnTrueWhileTheEngineRuns()
    {
        using KhaosEngine engine = TestEngine.Start(Game2D.Mock(1280, 720));

        bool pushed = EventQueue.TryPush(new PitStop(2));

        await pushed.Should().BeTrue();
    }

    private static bool IsLapThree(Event e) => e is CustomEvent custom && custom.TryGet(out LapCompleted lap) && lap.Number == 3;

    private readonly record struct BoostUsed(int Count);

    private readonly record struct CheckpointReached(int Index);

    private readonly record struct LapCompleted(int Number);

    private readonly record struct PitStop(int Lap);

    private readonly record struct RaceFinished(int Position);

    private readonly record struct RaceStarted(int Track);

    private readonly record struct SectorTimed(int Seconds);

    private readonly record struct TireChanged(int Tire);
}
