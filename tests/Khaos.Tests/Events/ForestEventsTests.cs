// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;
using KappaDuck.Khaos.Interop.SDL.Primitives;

namespace KappaDuck.Khaos.Tests.Events;

internal sealed class ForestEventsTests
{
    private const int GiveUpFrame = 10;

    [Test]
    [Category(Categories.Integration)]
    [NotInParallel(Sequential.Engine)]
    public void ForestEventsShouldReachOnEventThroughAPropertyPattern()
    {
        Mock<Game2D> game = Game2D.Mock(1280, 720);
        PushANestEachFrame(game);
        game.OnEvent(static received => received is CustomEvent { ForestEvents: NestFound { Eggs: 3 } }).Callback(game.Object.Exit);

        TestEngine.Run(game);

        game.Draw(Any(), Any()).WasCalled(Times.Once);
    }

    [Test]
    public async Task FromShouldHoldNoValueForAnEventOutsideTheGroup()
    {
        CustomEvent e = Waiting(new AcornDropped(2));

        ForestEvents forest = ForestEvents.From(in e);

        await forest.HasValue.Should().BeFalse();
    }

    [Test]
    public async Task FromShouldReadAnEventThatHoldsAString()
    {
        CustomEvent e = Waiting(new RangerNamed("Robin"));

        _ = ForestEvents.From(in e).TryGetValue(out RangerNamed named);

        await named.Should().BeEqualTo(new RangerNamed("Robin"));
    }

    [Test]
    public async Task FromShouldReadTheEventACustomEventCarries()
    {
        CustomEvent e = Waiting(new TreeFelled(4));

        _ = ForestEvents.From(in e).TryGetValue(out TreeFelled felled);

        await felled.Should().BeEqualTo(new TreeFelled(4));
    }

    private static void PushANestEachFrame(Mock<Game2D> game)
    {
        int draws = 0;

        game.Draw(Any(), Any()).Callback(() =>
        {
            draws++;
            _ = EventQueue.TryPush(new NestFound(3));

            if (draws == GiveUpFrame)
                game.Object.Exit();
        });
    }

    private static CustomEvent Waiting<T>(T payload) where T : struct
    {
        CustomEventSlots<T> slots = CustomEventStore<T>.GetOrRegister();
        _ = slots.TryAdd(in payload, out int code);

        SDL_UserEvent native = new(slots.Type, code);
        return new CustomEvent(in native);
    }

    private readonly record struct AcornDropped(int Count);
}
