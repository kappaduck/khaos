// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Tests;

internal sealed class PlatformExceptionTests
{
    [Test]
    public async Task ConstructorShouldHaveNoInnerExceptionByDefault()
    {
        PlatformException exception = new();

        await exception.InnerException.Should().BeNull();
    }

    [Test]
    public async Task ConstructorShouldKeepTheExceptionThatCausedIt()
    {
        InvalidOperationException cause = new();

        PlatformException exception = new("No display is available.", cause);

        await exception.InnerException.Should().BeSameReferenceAs(cause);
    }
}
