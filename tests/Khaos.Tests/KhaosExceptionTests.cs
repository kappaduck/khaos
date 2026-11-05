// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Tests;

internal sealed class KhaosExceptionTests
{
    [Test]
    public async Task ConstructorShouldHaveNoInnerExceptionByDefault()
    {
        KhaosException exception = new();

        await exception.InnerException.Should().BeNull();
    }

    [Test]
    public async Task ConstructorShouldKeepTheMessage()
    {
        KhaosException exception = new("The save file is corrupted.");

        await exception.Message.Should().BeEqualTo("The save file is corrupted.");
    }

    [Test]
    public async Task ConstructorShouldKeepTheExceptionThatCausedIt()
    {
        InvalidOperationException cause = new();

        KhaosException exception = new("The save file is corrupted.", cause);

        await exception.InnerException.Should().BeSameReferenceAs(cause);
    }
}
