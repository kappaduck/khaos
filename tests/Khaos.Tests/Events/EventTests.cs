// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;
using KappaDuck.Khaos.Input;
using System.Runtime.CompilerServices;

namespace KappaDuck.Khaos.Tests.Events;

internal sealed class EventTests
{
    [Test]
    public async Task DefaultShouldHoldNoValue()
    {
        Event e = default;

        await e.HasValue.Should().BeFalse();
    }

    [Test]
    public async Task DefaultValueShouldBeNull()
    {
        Event e = default;

        await e.Value.Should().BeNull();
    }

    [Test]
    public async Task HasValueShouldBeTrueForAnEventBuiltFromACase()
    {
        Event e = new(new QuitRequested());

        await e.HasValue.Should().BeTrue();
    }

    [Test]
    public async Task PropertyPatternShouldMatchTheValueItHolds()
    {
        Event e = new(new KeyPressed(Scancode.Escape));

        bool matched = e is KeyPressed { Code: Scancode.Escape };

        await matched.Should().BeTrue();
    }

    [Test]
    public async Task PropertyPatternShouldNotMatchAnotherValue()
    {
        Event e = new(new KeyPressed(Scancode.Space));

        bool matched = e is KeyPressed { Code: Scancode.Escape };

        await matched.Should().BeFalse();
    }

    [Test]
    public async Task SizeShouldStayTwelveBytes()
    {
        int size = Unsafe.SizeOf<Event>();

        await size.Should().BeEqualTo(12);
    }

    [Test]
    public async Task TryGetValueShouldGiveTheValueItHolds()
    {
        Event e = new(new KeyReleased(Scancode.A));

        _ = e.TryGetValue(out KeyReleased released);

        await released.Code.Should().BeEqualTo(Scancode.A);
    }

    [Test]
    public async Task TryGetValueShouldReturnFalseForAnotherCase()
    {
        Event e = new(new KeyReleased(Scancode.A));

        bool found = e.TryGetValue(out KeyPressed _);

        await found.Should().BeFalse();
    }

    [Test]
    public async Task TryGetValueShouldReturnTrueForTheCaseItHolds()
    {
        Event e = new(new WindowMinimized());

        bool found = e.TryGetValue(out WindowMinimized _);

        await found.Should().BeTrue();
    }

    [Test]
    public async Task TypePatternShouldMatchTheCaseItHolds()
    {
        Event e = new(new WindowFocusLost());

        bool matched = e is WindowFocusLost;

        await matched.Should().BeTrue();
    }
}
