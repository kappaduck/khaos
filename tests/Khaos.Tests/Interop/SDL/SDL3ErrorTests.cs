// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.SDL;

namespace KappaDuck.Khaos.Tests.Interop.SDL;

public sealed class SDL3ErrorTests
{
    [Test]
    public async Task ThrowIfFailedWhenConditionIsTrueShouldNotThrow()
    {
        await Assert.That(static () => SDL3.ThrowIfFailed(true))
                    .ThrowsNothing();
    }

    [Test]
    public async Task ThrowIfFailedWhenConditionIsFalseShouldThrow()
    {
        await Assert.That(static () => SDL3.ThrowIfFailed(false))
                    .ThrowsExactly<KhaosInteropException>();
    }

    [Test]
    public async Task ThrowIfNegativeWhenValueIsNotNegativeShouldNotThrow()
    {
        await Assert.That(static () => SDL3.ThrowIfNegative(42))
                    .ThrowsNothing();
    }

    [Test]
    public async Task ThrowIfNegativeShouldReturnValueWhenValueIsNotNegative()
    {
        const int value = 42;

        int returned = SDL3.ThrowIfNegative(value);

        await returned.Should().BeEqualTo(value);
    }

    [Test]
    public async Task ThrowIfNegativeWhenValueIsNegativeShouldThrow()
    {
        await Assert.That(static () => SDL3.ThrowIfNegative(-12))
                    .ThrowsExactly<KhaosInteropException>();
    }

    [Test]
    public async Task ThrowIfNullWhenValueIsNotNullShouldNotThrow()
    {
        await Assert.That(static () => SDL3.ThrowIfNull("text"))
                    .ThrowsNothing();
    }

    [Test]
    public async Task ThrowIfNullShouldReturnValueWhenValueIsNotNull()
    {
        const string value = "text";

        string returned = SDL3.ThrowIfNull(value);

        await returned.Should().BeEqualTo(value);
    }

    [Test]
    public async Task ThrowIfNullWhenValueIsNullShouldThrow()
    {
        const string? value = null;

        await Assert.That(static () => SDL3.ThrowIfNull(value))
                    .ThrowsExactly<KhaosInteropException>();
    }

    [Test]
    public async Task ThrowIfZeroWhenValueIsNotZeroShouldNotThrow()
    {
        await Assert.That(static () => SDL3.ThrowIfZero(42))
                    .ThrowsNothing();
    }

    [Test]
    public async Task ThrowIfZeroShouldReturnValueWhenValueIsNotZero()
    {
        const int value = 42;

        int returned = SDL3.ThrowIfZero(value);

        await returned.Should().BeEqualTo(value);
    }

    [Test]
    public async Task ThrowIfZeroWhenValueIsZeroShouldThrow()
    {
        await Assert.That(static () => SDL3.ThrowIfZero(0))
                    .ThrowsExactly<KhaosInteropException>();
    }
}
